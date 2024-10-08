﻿using System.Globalization;
using System.Text;

namespace COBE_CS {
    internal class Compiler {
        string prefix = "";
        string[] asm_content = { };
        public int cur_line = 0;
        string title = "Program";

        List<Byte> header = new List<byte>();
        List<Byte> imps = new List<byte>();
        public List<Byte> inst = new List<byte>();

        bool ret_exists = false;
        bool ret_correct = false;
        bool header_exists = false;
        bool header_correct = false;
        bool custom_title = false;
        bool has_start = false;
        bool impscr = false;
        public bool flag_err = false;

        public void PrintA(string type, char[] bytes) {
            var sb = new StringBuilder(type + " compiled as byte { ");
            int index = 0;
            foreach (var b in bytes) {
                sb.Append(b);
                if (bytes.Length - 1 > index) sb.Append(", ");
                index += 1;
            }
            sb.Append(" }");
            Console.WriteLine(sb.ToString());
        }

        // ------------------- Conversion Functions ---------------------
        public byte[] intToBa(int num) {
            string s = num.ToString("X");
            List<Byte> hex = new List<byte>();

            // "320" -> "0320" -> [ 03, 20 ]
            string zeros = "";
            for (int i = 4 - s.Length; i > 0; i--) {
                zeros += "0";
            }
            s = zeros + s;

            // The number is now 4 long, split it in half.
            hex.Add(Byte.Parse(s[0..2], NumberStyles.HexNumber));
            hex.Add(Byte.Parse(s[2..4], NumberStyles.HexNumber));

            return hex.ToArray();
        }

        public byte[] strToBa(string s) {
            List<Byte> intStr = new List<byte>();

            int position = 0;
            bool next = false;
            foreach (char letter in s) {
                if (next) { next = false; position += 1; continue; }

                int value = Convert.ToInt32(letter);

                if (letter == '\\') {
                    switch (s[position + 1]) {
                        case 't':
                            next = true;
                            value = 9;
                            break;
                        case 'n':
                            next = true;
                            value = 10;
                            break;
                        default:
                            throw new Exception("UNKNOWN_ESCAPE_SEQUENCE: " +
                                letter + s[position + 1]);
                    }

                }

                intStr.Add((byte)value);
                position += 1;
            }
            return intStr.ToArray();
        }

        public byte opToBy(string op) {
            switch (op) {
                case "+": return 1;
                case "-": return 2;
                case "*": return 3;
                case "/": return 4;
                case "=": return 5;
                case ">": return 10;
                case "<": return 11;
                case ">=": return 12;
                case "<=": return 13;
                case "!=": return 14;
                case "==": return 15;
            }
            return 0;
        }

        public byte boolToBy(string b) {
            if (b == "true") return 2;
            else if (b == "false") return 1;
            else return 0;
        }

        public byte[] colToBa(string s) {
            s = s.Replace("#",""); // #FF00FF -> FF00FF
            byte r = Convert.ToByte(s[0..2], 16);
            byte g = Convert.ToByte(s[2..4], 16);
            byte b = Convert.ToByte(s[4..6], 16);
            return [r, g, b]; // FF00FF -> 255,0,255
        }

        // ------------------- Assembly Load ---------------------
        public void LoadASM(string path) {
            asm_content = File.ReadAllLines(path);
        }

        // ------------------- Compilation ---------------------
        public void Compile() {
            foreach (var line in asm_content) {
                string[] con = line.Split(' ');

                switch (con[0].ToLower()) {
                    case "header:": {
                        byte mode = 0;
                        header_exists = true;

                        if (con[1].ToLower() == "terminal") mode = 1;
                        else if (con[1].ToLower() == "graphical") mode = 0;
                        else if (con[1].ToLower() == "library") mode = 2;
                        else throw new Exception("UNKNOWN_MODE");

                        byte[] width = [0, 0];
                        byte[] height = [0, 0];

                        if (mode != 2) {
                            width = intToBa(Int32.Parse(con[2]));
                            height = intToBa(Int32.Parse(con[3]));
                        }

                        header.Add(mode);
                        header.AddRange(width);
                        header.AddRange(height);
                        header.AddRange([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

                        header_correct = true;
                        break;
                    }

                    case "title:": {
                        string s = string.Join(" ", con[1..]).Replace("\"", "");
                        if (s.Length > 32) s = s[..32];

                        List<byte> zeros = new List<byte>();
                        for (int i = 32 - s.Length; i > 0; i--) {
                            zeros.Add(0);
                        }

                        header.AddRange(strToBa(s));
                        header.AddRange(zeros);
                        custom_title = true;
                        break;
                    }

                    case String s when string.IsNullOrWhiteSpace(s) || s.StartsWith(";"): {
                        break;
                    }

                    case "nop": {
                        inst.Add(0);
                        break;
                    }

                    case "lbl": {
                        // Label Type
                        byte lbl_type = 0;
                        string lbl_con = String.Join(" ", con[3..]);
                        byte[] lcon = [];

                        switch (con[1].ToLower()) {
                            case "boolean":
                                lbl_type = 1;
                                lcon = [boolToBy(lbl_con)];
                                break;

                            case "string":
                                lbl_type = 2;
                                lcon = strToBa(lbl_con);
                                break;

                            case "number":
                                lbl_type = 3;
                                lcon = intToBa(int.Parse(lbl_con));
                                break;

                            case "color":
                                lbl_type = 4;
                                lcon = colToBa(lbl_con);
                                break;

                            case "file":
                                break;

                            default:
                                throw new Exception("UNKNOWN_LABEL_TYPE");
                        }

                        List<byte> ninst = [1, lbl_type, ..strToBa(prefix + con[2]), 0];
                        ninst.AddRange(lcon);
                        ninst.Add(0);

                        inst.AddRange(ninst);
                        break;
                    }

                    case "mth": {
                        if (con[1].All(char.IsDigit)) throw new Exception("LABEL_ARG_IS_NUMERIC");

                        byte[] num1 = strToBa(con[1]);
                        byte[] num2 = strToBa(con[3]);
                        byte numt = 6;

                        if (con[3].All(char.IsDigit)) {
                            num2 = intToBa(Int32.Parse(con[3]));
                            numt = 3;
                        } else if (con[3].Contains("\"")) {
                            num2 = strToBa(string.Join(" ", con[3..]));
                            numt = 2;
                        }
                        byte op = opToBy(con[2]);

                        List<byte> ninst = [2, op, .. num1, 0, numt, .. num2, 0];

                        inst.AddRange(ninst);
                        break;
                    }

                    case "put": {
                        byte type = 6;
                        byte[] pcon = strToBa(con[1]);

                        if (con[1].Contains("\"")) {
                            type = 2;
                            pcon = strToBa(String.Join(" ", con[1..]));
                        } else if (con[1] == "true" || con[1] == "false") {
                            type = 1;
                            pcon = new byte[] { boolToBy(con[1]) };
                        } else if (con[1].StartsWith("#")) {
                            type = 4;
                            pcon = strToBa(con[1].Replace("#", ""));
                        } else if (con[1].All(char.IsDigit)) {
                            type = 3;
                            pcon = intToBa(Int32.Parse(con[1]));
                        }

                        List<byte> ninst = [3, type, .. pcon, 0];

                        inst.AddRange(ninst);
                        break;
                    }

                    case "rki": {
                        if (con.Length > 2) throw new Exception("RKI_TOO_LONG");
                        if (con[1].All(char.IsDigit)) throw new Exception("NUMERIC_AS_LABEL");
                        if (con[1].StartsWith("\"")) throw new Exception("STRING_AS_LABEL");

                        List<byte> ninst = [4, .. strToBa(con[1]), 0];

                        inst.AddRange(ninst);
                        break;
                    }

                    case "ret": {
                        ret_exists = true;

                        byte type = 6;
                        byte[] ret_code = strToBa(con[1]);
                        if (con[1].All(char.IsDigit)) {
                            type = 3;
                            ret_code = intToBa(Int32.Parse(con[1]));
                        }

                        List<byte> ninst = [5, type, ..ret_code, 0];

                        inst.AddRange(ninst);
                        ret_correct = true;
                        break;
                    }

                    case "mrk": {
                        if (con.Length > 2) throw new Exception("MRK_TOO_LONG");
                        if (con[1].All(char.IsDigit)) throw new Exception("NUMERIC_AS_MARKER");
                        if (con[1].StartsWith("\"")) throw new Exception("STRING_AS_MARKER");

                        List<byte> ninst = [];
                        if (con[1] == "_start") {
                            ninst.AddRange([15, 1]);
                            has_start = true;
                        } else ninst.AddRange([6, ..strToBa(con[1]), 0]);

                        inst.AddRange(ninst);
                        break;
                    }

                    case "jmp": {
                        if (con.Length > 2) throw new Exception("JMP_TOO_LONG");
                        if (con[1].All(char.IsDigit)) throw new Exception("NUMERIC_AS_MARKER");
                        if (con[1].StartsWith("\"")) throw new Exception("STRING_AS_MARKER");

                        List<byte> ninst = [7, ..strToBa(con[1]), 0];
                        inst.AddRange(ninst);
                        break;
                    }

                    case "ssc": {
                        byte[] num1 = strToBa(con[1]), num2 = strToBa(con[2]);
                        byte num1t = 6, num2t = 6;

                        if (con[1].All(char.IsDigit)) {
                            num1 = intToBa(Int32.Parse(con[1]));
                            num1t = 3;
                        }

                        if (con[2].All(char.IsDigit)) {
                            num2 = intToBa(Int32.Parse(con[2]));
                            num2t = 3;
                        }

                        List<byte> ninst = [8, num1t, ..num1, 0, num2t, .. num2, 0];
                        inst.AddRange(ninst);
                        break;
                    }

                    case "bep": {
                        byte[] num1 = strToBa(con[1]), num2 = strToBa(con[2]);
                        byte num1t = 6, num2t = 6;

                        if (con[1].All(char.IsDigit)) {
                            num1 = intToBa(Int32.Parse(con[1]));
                            num1t = 3;
                        }

                        if (con[2].All(char.IsDigit)) {
                            num2 = intToBa(Int32.Parse(con[2]));
                            num2t = 3;
                        }

                        List<byte> ninst = [9, num1t, ..num1, 0, num2t, ..num2, 0];
                        inst.AddRange(ninst);
                        break;
                    }

                    case "ifj": {
                        byte[] num1 = strToBa(con[1]), num2 = strToBa(con[3]);
                        byte[] marker = strToBa(con[4]);
                        byte num1t = 6, num2t = 6, op = opToBy(con[2]);

                        if (con[1].All(char.IsDigit)) {
                            num1 = intToBa(Int32.Parse(con[1]));
                            num1t = 3;
                        } else if (con[1] == "true" || con[1] == "false") {
                            num1t = 1;
                            num1 = new byte[] { boolToBy(con[1]) };
                        }

                        if (con[3].All(char.IsDigit)) {
                            num2 = intToBa(Int32.Parse(con[3]));
                            num2t = 3;
                        } else if (con[1] == "true" || con[1] == "false") {
                            num2t = 1;
                            num2 = new byte[] { boolToBy(con[1]) };
                        }

                        List<byte> ninst = [10, op, num1t, ..num1, 0, num2t, ..num2, 0, ..marker, 0];
                        inst.AddRange(ninst);
                        break;
                    }

                    case "dtb": {
                        byte[] num1 = strToBa(con[1]), num2 = strToBa(con[2]);
                        byte[] col = strToBa(con[3]);
                        byte num1t = 6, num2t = 6, type = 6;

                        if (con[3].StartsWith("#")) {
                            type = 4;
                            col = strToBa(con[2].Replace("#", ""));
                        }

                        if (con[1].All(char.IsDigit)) {
                            num1 = intToBa(Int32.Parse(con[1]));
                            num1t = 3;
                        }

                        if (con[2].All(char.IsDigit)) {
                            num2 = intToBa(Int32.Parse(con[2]));
                            num2t = 3;
                        }

                        List<byte> ninst = [11, num1t, ..num1, 0, num2t, ..num2, 0, type, ..col, 0];
                        inst.AddRange(ninst);
                        break;
                    }

                    case "cdb": {
                        byte type = 6;
                        byte[] num = strToBa(con[2].Replace("#", ""));

                        List<byte> ninst = [12, type, ..num, 0];
                        inst.AddRange(ninst);
                        break;
                    }

                    case "rfb": {
                        byte[] num1 = strToBa(con[1]), num2 = strToBa(con[2]);
                        byte num1t = 6, num2t = 6;

                        if (con[1].All(char.IsDigit)) {
                            num1 = intToBa(Int32.Parse(con[1]));
                            num1t = 3;
                        }

                        if (con[2].All(char.IsDigit)) {
                            num2 = intToBa(Int32.Parse(con[2]));
                            num2t = 3;
                        }

                        List<byte> ninst = [13, num1t, ..num1, 0, num2t, ..num2, 0];
                        inst.AddRange(ninst);
                        break;
                    }

                    case "wft": {
                        byte[] num = strToBa(con[1]);
                        byte numt = 6;

                        if (con[1].All(char.IsDigit)) {
                            num = intToBa(Int32.Parse(con[1]));
                            numt = 3;
                        }

                        List<byte> ninst = [ 14, numt, ..num, 0];

                        inst.AddRange(ninst);
                        break;
                    }

                    case "imp": {
                        string impscript = con[1].Replace("/", ".");

                        Compiler imp_comp = new Compiler();
                        imp_comp.prefix = impscript + ".";

                        // Attempt to compile the imported script
                        imp_comp.LoadASM(impscript + ".asm");
                        imp_comp.Compile();

                        // Merge with the main script
                        imps.AddRange(imp_comp.inst);
                        impscr = true;
                        break;
                    }

                    case "rtj": {
                        if (con.Length > 2) throw new Exception("MRK_TOO_LONG");
                        if (con[1].All(char.IsDigit)) throw new Exception("NUMERIC_AS_MARKER");
                        if (con[1].StartsWith("\"")) throw new Exception("STRING_AS_MARKER");

                        List<byte> ninst = [17, .. strToBa(con[1]), 0];

                        inst.AddRange(ninst);
                        break;
                    }

                    default:
                        throw new Exception("UNKNOWN_INST: " + con[0].ToLower());
                }

                cur_line++; 
            }
        }

        // Check if everything is in order
        public void CheckFlags() {
            flag_err = true;
            if (!header_exists) throw new Exception("HEADER_NOT_PRESENT");
            if (!header_correct) throw new Exception("HEADER_NOT_CORRECT");

            if (!ret_exists) throw new Exception("RETURN_NOT_PRESENT");
            if (!ret_correct) throw new Exception("RETURN_NOT_PRESENT");

            if (!custom_title) {
                string s = string.Join(" ", title).Replace("\"", "");

                List<byte> zeros = new List<byte>();
                for (int i = 32 - s.Length; i > 0; i--) {
                    zeros.Add(0);
                }
                header.AddRange(strToBa(s));
                header.AddRange(zeros);
            }

            if (has_start) {
                imps.AddRange([15,2]);
            }

            if (impscr) {
                imps.InsertRange(0,[15,2]);
                imps.AddRange([15,1]);
            }
        }

        // ------------------- Write to binary file ---------------------

        public void WriteToBIN(string path) {
            if (File.Exists(path)) {
                File.Delete(path);
            }

            BinaryWriter binfile = new BinaryWriter(File.OpenWrite(path));
            binfile.Write(header.ToArray());
            binfile.Write(imps.ToArray());
            binfile.Write(inst.ToArray());
            binfile.Close();
        }
    }

    class CompilerProgram {
        static Compiler compiler = new Compiler();

        public byte Main(string[] args) { // exe.exe test.asm app.cae
            string infile = "test.asm";
            string outfile = "app.cae";

            try {
                if (args.Length >= 1) infile = args[0];
                if (args.Length >= 2) outfile = args[1];
                if (args.Length >= 3) Console.WriteLine("Too many arguments, ignoring leftover.");
            } catch (Exception ex) { Console.WriteLine(ex.Message); return 1; }

            try {
                compiler.LoadASM(infile);
                compiler.Compile();
                compiler.CheckFlags();
                compiler.WriteToBIN(outfile);

                return 0;
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                if (!compiler.flag_err) Console.WriteLine("Line: " + compiler.cur_line);
                Console.WriteLine("File: " + infile);

                return 1;
            }
        }
    }
}