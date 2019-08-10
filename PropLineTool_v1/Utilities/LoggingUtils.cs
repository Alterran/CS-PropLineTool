using UnityEngine;

namespace PropLineTool.LoggingUtils {
    public static class Output {
        public const string MOD_PREFIX = "PLT";

        private static string FullMessagePrefix(string basePrefix, bool debugMessage) {
            return "[" + basePrefix + (debugMessage ? "_DEBUG]: " : "]: ");
        }

        public static void LogSimple(string message) {
            Debug.Log(message);
        }
        public static void Log(string message) {
            UnityEngine.Debug.Log(FullMessagePrefix(MOD_PREFIX, false) + message);
        }
        public static void Log(string message, bool debug) {
            UnityEngine.Debug.Log(FullMessagePrefix(MOD_PREFIX, true) + message);
        }
        public static void LogDebug(string message) {
            Log(message, true);
        }
    }
}