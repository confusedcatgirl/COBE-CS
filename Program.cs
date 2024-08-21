
namespace COBE_CS {
    class Program {
        public static void Main(string[] args) {
            if (args[0] == "compile") {
                if (args.Length < 3) throw new Exception("Missing arguments!");

                CompilerProgram comprg = new CompilerProgram();
                comprg.Main([args[1],args[2]]);
            } else if (args[0] == "emulate") {
                if (args.Length < 2) throw new Exception("Missing arguments!");

                EmulatorProgram emuprg = new EmulatorProgram();
                emuprg.Main([args[1]]);
            }
        }
    }
}