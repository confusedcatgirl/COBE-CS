
using System.Numerics;
using System.Text;

namespace COBE_CS {
    class Compiler {
        string[]? asm_content;
        string[]? content;
        string[]? bin_content;

        byte[] header = new byte[5];

        public void PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            Console.WriteLine(sb.ToString());
        }
    
        public byte[] intToHex(int num) {
            bool loop = true;

            while (loop) {
                int div = (int)Math.Round((double)num / 16);
                int rest = num % 16;

                List<byte> result = new List<byte>();

                if (div < 16) {
                    loop = false;
                }
                num = div;

            }

            return new byte[4];
        }

        public void LoadASM(string path) {
            asm_content = File.ReadAllLines(path);
            content = new string[asm_content.Length];


            int cur_line = 1;
            foreach (var line in asm_content) {
                if (line.Replace(" ", "") != "") {
                    content.Append(line);

                    if (line.ToLower().Contains("header")) { // Parse Headers
                        string[] con = line.ToLower().Split(' ');
                        byte mode;

                        if (con[1].ToLower() == "terminal") mode = 1;
                        else if(con[1].ToLower() == "graphical") mode = 0;

                        byte[]? width = intToHex(Int32.Parse(con[2]));
                        byte[]? height = BitConverter.GetBytes(Int32.Parse(con[3]));

                        
                    }

                    cur_line++;
                }
            }
        }

        public void ParseHeader() {

        }
    }

    class Program {
        static Compiler? compiler;
        public static void Main(string[] args) {
            compiler = new Compiler();
            compiler.LoadASM("test.asm");
        }
    }
}