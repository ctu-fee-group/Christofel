using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Christofel.CommandsLib.Verifier.Verifiers
{
    public static class FileVerifierExtensions
    {
        public const string FileRegex = @"^[a-zA-Z0-9_\-\.]+$";
        
        /// <summary>
        /// Verifies if fileName is present in the filesystem.
        /// Also verifies its name against <see cref="FileRegex"/>
        /// so folder cannot be selected
        /// </summary>
        /// <param name="verifier"></param>
        /// <param name="rootPath">Folder the file should be located in</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="extension">Extension of the file that will be applied as .{extension} to the fileName</param>
        /// <param name="parameterName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CommandVerifier<T> VerifyFile<T>(this CommandVerifier<T> verifier,
            string rootPath, string fileName, string extension,
            string parameterName = "messageid")
            where T : new()
        {
            verifier.QueueWork(() => verifier.VerifyFileAsync(rootPath, fileName, extension, parameterName));
            return verifier;
        }

        private static Task<CommandVerifier<T>> VerifyFileAsync<T>(this CommandVerifier<T> verifier,
            string rootPath, string fileName, string extension, string parameterName = "messageid")
            where T : new()
        {
            if (!Regex.IsMatch(fileName, FileRegex))
            {
                verifier.SetFailed(parameterName, "File name cannot be accepted.");
                return Task.FromResult(verifier);
            }

            if (!File.Exists(Path.Join(rootPath, fileName + extension)))
            {
                verifier.SetFailed(parameterName, "File not found.");
                return Task.FromResult(verifier);
            }
            
            return Task.FromResult(verifier);
        }
    }
}