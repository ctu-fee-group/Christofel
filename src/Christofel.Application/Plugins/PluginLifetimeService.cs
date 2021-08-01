using System;
using System.Threading;
using System.Threading.Tasks;
using Christofel.Application.Assemblies;
using Christofel.Application.Extensions;
using Christofel.BaseLib;
using Christofel.BaseLib.Lifetime;
using Christofel.BaseLib.Plugins;
using Microsoft.Extensions.Logging;

namespace Christofel.Application.Plugins
{
    /// <summary>
    /// Service for handling plugin lifetime
    /// </summary>
    public class PluginLifetimeService
    {
        private readonly ILogger _logger;
        private readonly PluginStorage _storage;
        
        public PluginLifetimeService(
            ILogger<PluginLifetimeService> logger,
            PluginStorage storage)
        {
            _storage = storage;
            _logger = logger;
        }
        
        /// <summary>
        /// Load plugin Assembly to memory
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<ContextedAssembly> AttachAssemblyAsync(string path, CancellationToken token = new CancellationToken())
        {
            _logger.LogDebug($@"Loading assembly");
            ContextedAssembly info = AssemblyLoader.Load(path);
            _logger.LogDebug($@"Assembly loaded");
            
            if (token.IsCancellationRequested)
            {
                info.Detach();
                token.ThrowIfCancellationRequested();
            }

            return Task.FromResult(info);
        }
        
        /// <summary>
        /// Creates raw plugin instance
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<IPlugin> CreateRawPluginAsync(ContextedAssembly info)
        {
            _logger.LogDebug($@"Finding IModule");
            Type pluginType = info.Assembly.GetTypeImplementing<IPlugin>();
            _logger.LogDebug($@"Found IModule");

            IPlugin? rawPlugin = (IPlugin?) Activator.CreateInstance(pluginType);
            if (rawPlugin == null)
            {
                _logger.LogDebug($@"Plugin could not be initialized");
                throw new InvalidOperationException("Could not initialize the plugin");
            }
            
            return Task.FromResult(rawPlugin);
        }
        
        /// <summary>
        /// Initializes and starts a plugin along with registering lifetime callbacks
        /// </summary>
        /// <param name="rawPlugin"></param>
        /// <param name="info"></param>
        /// <param name="state"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<AttachedPlugin> InitializePluginAsync(IPlugin rawPlugin, ContextedAssembly info, IChristofelState state, CancellationToken token = new CancellationToken())
        {
            AttachedPlugin plugin = new AttachedPlugin(rawPlugin, info);
            _storage.AddAttachedPlugin(plugin);

            ILifetime lifetime = rawPlugin.Lifetime;
            RegisterLifetimeCallbacks(lifetime, plugin);

            _logger.LogDebug($@"Initialization started");
            await rawPlugin.InitAsync(state, token);
            if (token.IsCancellationRequested)
            {
                info.Detach();
                rawPlugin.Lifetime.RequestStop();
                token.ThrowIfCancellationRequested();
            }

            await rawPlugin.RunAsync(token);
            
            _logger.LogInformation($@"Plugin {plugin} was initialized successfully");

            return plugin;
        }
        
        /// <summary>
        /// Tries to stop plugin in time given.
        /// If it fails, it will register callback for stop.
        /// </summary>
        /// <param name="detached"></param>
        /// <param name="lifetime"></param>
        /// <param name="timeout"></param>
        public async Task TryStopPluginAsync(DetachedPlugin detached, ILifetime lifetime, int timeout = 10000)
        {
            _logger.LogInformation($@"Requesting stop for plugin {detached}, will wait at most 10 seconds for it to stop");
            lifetime.RequestStop();
            
            if (!await lifetime.WaitForAsync(LifetimeState.Stopped, timeout))
            {
                _logger.LogWarning($@"Plugin {detached} does not respond to stop request, attaching for late destroy");
                lifetime.Stopped.Register(() =>
                {
                    detached.DestroyedLate = true;
                    _logger.LogInformation($@"Plugin finally {detached} stopped late.");
                });
            }
            else if (!await lifetime.WaitForAsync(LifetimeState.Destroyed, timeout / 5))
            {
                _logger.LogWarning($@"Plugin {detached} stopped, but did not destroy itself in time. Lost it's track.");
            }
            else
            {
                detached.DestroyedInTime = true;
                _logger.LogInformation($@"Plugin {detached} was stopped and destroyed in time given!");
            }
        }

        /// <summary>
        /// Registers lifetime callbacks to allow plugin detach
        /// without calling detach from the application
        /// </summary>
        /// <param name="lifetime"></param>
        /// <param name="plugin"></param>
        public void RegisterLifetimeCallbacks(ILifetime lifetime, AttachedPlugin plugin)
        {
            lifetime.Errored.Register(() =>
            {
                _logger.LogError($@"Received error state from {plugin}. Going to detach it");

                if (plugin.DetachedPlugin != null)
                {
                    _logger.LogInformation($@"Plugin {plugin.DetachedPlugin} was already detached and this means that it was probably not well exited");
                }
                else
                {
                    DetachPluginAsync(plugin, null)
                        .GetAwaiter()
                        .GetResult();
                }
            });

            lifetime.Stopped.Register(() =>
            {
                if (plugin.DetachedPlugin == null)
                {
                    _logger.LogWarning($@"Plugin {plugin} reported stopped, but it was not requested, detaching it");
                    try
                    {
                        DetachPluginAsync(plugin, null)
                            .GetAwaiter()
                            .GetResult();
                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical(e, "Plugin {plugin} errored during detaching");
                    }
                }
                else
                {
                    _logger.LogInformation($@"Plugin {plugin.DetachedPlugin} reported stopped and was detached/is being detached, not handling it in callback");
                }
            });
        }

        /// <summary>
        /// Tries to detach plugin from memory, if it isn't destroyed, tries to stop it as well
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="detached"></param>
        /// <returns></returns>
        public async Task<DetachedPlugin> DetachPluginAsync(AttachedPlugin plugin, DetachedPlugin? detached)
        {
            if (detached == null)
            {
                detached = plugin.DetachedPlugin = new DetachedPlugin(plugin);
                _storage.DetachAttachedPlugin(plugin);
            }

            ILifetime lifetime = plugin.Plugin.Lifetime;
            if (lifetime.State < LifetimeState.Destroyed)
            { 
                await TryStopPluginAsync(detached, lifetime);
            }
            
            WeakReference reference = plugin.Detach();
            detached.AssemblyContextReference = reference;

            return detached;
        }
    }
}