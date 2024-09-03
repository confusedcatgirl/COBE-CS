
namespace COBE_CS {
	class Program {
        static void Main(string[] args) {
			//try {
				if (args.Length < 2) throw new ArgumentException("Too few Arguments!");

				if (args[0] == "emulate") {
					// Emulate the given Binary File
					if (args.Length < 2)
						throw new ArgumentException("Too few Arguments!");

					EmulatorProgram emuprg = new EmulatorProgram();
					emuprg.Main([args[1]]);

				} else if (args[0] == "compile") {
					// Compile the given assembly file into binary
					if (args.Length < 3)
						throw new ArgumentException("Too few Arguments!");

					CompilerProgram comprg = new CompilerProgram();
					comprg.Main(args[1..3]);

				} 
			//} catch (Exception e) { Console.WriteLine(e.Message); }
			//Console.ReadKey();
		}
	}
}