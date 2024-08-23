
namespace COBE_CS {
    class Program {
        public static bool IsFileReady(string filename) {
            try {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputStream.Length > 0;
            } catch (Exception) { return false; }
        }

        public static void WaitForFile(string filename) {
            //This will lock the execution until the file is ready
            //TODO: Add some logic to make it async and cancelable
            while (!IsFileReady(filename)) { }
        }

        public static void Main(string[] args) {
            if (args.Length == 0) throw new Exception("Missing arguments!");
            
            if (args[0] == "compile") {
                if (args.Length < 3) throw new Exception("Missing arguments!");

                CompilerProgram comprg = new CompilerProgram();
                comprg.Main([args[1],args[2]]);

            } else if (args[0] == "emulate") {
                if (args.Length < 2) throw new Exception("Missing arguments!");

                EmulatorProgram emuprg = new EmulatorProgram();
                emuprg.Main([args[1]]);

            } else if (args[0] == "run") {
                if (args.Length < 3) throw new Exception("Missing arguments!");

                CompilerProgram comprg = new CompilerProgram();
                EmulatorProgram emuprg = new EmulatorProgram();

                comprg.Main([args[1],args[2]]);
                WaitForFile(args[2]);
                emuprg.Main([args[2]]);
            }
        }
    }
}