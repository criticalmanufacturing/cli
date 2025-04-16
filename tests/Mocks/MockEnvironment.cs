using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace tests.Mocks
{
    /// <summary>
    /// Mock class for the Environment static .NET Class. For now, it simply sets the environment variables on the real Environment class,
    /// and provides a method to restore the original values at the end of the test.
    /// 
    /// In the future, the goal is to replace all references to the Environment static class with an interface, and have this class implement said interface instead,
    /// providing mockinf facilities not just for environment variables, but all other methods of the Environment class.
    /// </summary>
    public class MockEnvironment
    {
        private Dictionary<string, string?> OriginalAttributeValues { get; set; }

        public MockEnvironment()
        {
            OriginalAttributeValues = new Dictionary<string, string?>();
        }

        public MockEnvironment(Dictionary<string, string> envVars)
        {
            OriginalAttributeValues = new Dictionary<string, string?>();
            
            foreach (var pair in envVars)
            {
                SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Retrieves the value of an environment variable
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public string? GetEnvironmentVariable(string variable) 
        {
            return Environment.GetEnvironmentVariable(variable);
        }

        /// <summary>
        /// Set the value of an environment variable
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        public void SetEnvironmentVariable(string variable, string? value)
        {
            // If we are changing the value of the same variable for a second or nth time, we don't want to
            // overwrite the original value we had already saved, so we only do it when the dictionary does not contain this variable yet
            if (!OriginalAttributeValues.ContainsKey(variable)) 
            {
                OriginalAttributeValues[variable] = Environment.GetEnvironmentVariable(variable);
            }

            Environment.SetEnvironmentVariable(variable, value);
        }

        /// <summary>
        /// Restore the value of the environment variables, as they were before modifying them through this class
        /// </summary>
        public void Restore()
        {
            foreach (var pair in OriginalAttributeValues) 
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }
    }
}

#nullable disable