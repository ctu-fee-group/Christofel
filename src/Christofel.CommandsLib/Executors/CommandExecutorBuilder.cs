using System;
using System.Collections.Generic;
using Christofel.BaseLib.Permissions;
using Microsoft.Extensions.Logging;

namespace Christofel.CommandsLib.Executors
{
    public class CommandExecutorBuilder
    {
        private bool _defer, _permissionsCheck, _threadPool;

        private ICommandExecutor? _base;
        private IPermissionsResolver? _resolver;
        private string? _deferMessage;
        private ILogger? _logger;

        /// <summary>
        /// Add PermissionCheckCommandExecutor decorator
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns>this</returns>
        public CommandExecutorBuilder WithPermissionsCheck(IPermissionsResolver resolver)
        {
            _permissionsCheck = true;
            _resolver = resolver;
            return this;
        }

        /// <summary>
        /// Add AutoDeferCommandExecutor decorator
        /// </summary>
        /// <param name="message">Message to respond with, if null, defer will be calle</param>
        /// <returns>this</returns>
        public CommandExecutorBuilder WithDeferMessage(string? message = "I am thinking...")
        {
            _defer = true;
            _deferMessage = message;
            return this;
        }
        
        /// <summary>
        /// Logger to initialize ThreadPoolCommandExecutor or base executor
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public CommandExecutorBuilder WithLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        /// <summary>
        /// Sets the underlying executor that will be decorated
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public CommandExecutorBuilder SetBaseExecutor(ICommandExecutor executor)
        {
            _base = executor;
            return this;
        }

        /// <summary>
        /// Add ThreadPoolCommandExecutor decorator
        /// </summary>
        /// <returns></returns>
        public CommandExecutorBuilder WithThreadPool()
        {
            _threadPool = true;
            return this;
        }

        /// <summary>
        /// Creates ICommandExecutor based on configuration of builder
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual ICommandExecutor Build()
        {
            if (_base is null)
            {
                if (_logger is null)
                {
                    throw new InvalidCastException("Logger must not be null");
                }
                
                _base = new CommandExecutor(_logger);
            }

            ICommandExecutor executor = _base;

            if (_threadPool)
            {
                if (_logger is null)
                {
                    throw new InvalidOperationException("Logger must not be null");
                }
                
                executor = new ThreadPoolCommandExecutor(_logger, executor);
            }

            if (_defer)
            {
                executor = new AutoDeferCommandExecutor(executor, _deferMessage);
            }

            if (_permissionsCheck)
            {
                if (_resolver == null)
                {
                    throw new InvalidOperationException("Permission resolver cannot be null if permission check should be enabled");
                }
                
                executor = new PermissionCheckCommandExecutor(executor, _resolver);
            }

            return executor;
        }
    }
}