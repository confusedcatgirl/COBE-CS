
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Reflection;
using System.Text;

#pragma warning disable CS8622 // Nullability of reference types
#pragma warning disable CS8604 // Possible null reference argument
#pragma warning disable CS8597 // Thrown value may be null

namespace COBE_CS {
    class OperationCodes {
        private GlobalData global;
        public List<Delegate> codes = new List<Delegate>();
        List<Delegate> normal_opc = new List<Delegate>();
        List<Delegate> imp_opc = new List<Delegate>();
        // https://stackoverflow.com/questions/42159436/store-methods-in-an-array-and-call-them-in-c-sharp

        public OperationCodes(GlobalData globdata) {
            normal_opc.AddRange([NOP, LBL, MTH, PUT, RKI, RET, MRK, JMP, SSC, BEP, IFJ, DTB, CDB, RFB, WFT, IMP,
                                 RTJ, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                                 NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP]);
            imp_opc.AddRange([NOP, LBL, NOP, NOP, NOP, NOP, MRK, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, IMP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                              NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP]);
            codes = normal_opc;
            global = globdata;
        }

        public string baToStr(byte[] encstr) {
            return Encoding.Default.GetString(encstr);
        }

        public int baToInt(byte[] ba) {
            int result = 0;
            int multiplier = 1;

            for (int i = ba.Length - 1; i >= 0; i--) {
                result += ba[i] * multiplier;
                multiplier *= 256;
            }

            return result;
        }

        public uint baToCol(byte[] col) {
            byte r = col[0], g = col[1], b = col[2];
            return (uint)((r << 16) | (g << 8) | (b << 0));
        }


        private int NOP(int code_pos, byte[] code) {
            Console.WriteLine("NOP: "+code[code_pos]+" - "+baToStr([code[code_pos]]));
            code_pos += 1;
            return code_pos;
        }

        private int LBL(int code_pos, byte[] code) {
            dynamic lbl_content = ""; string lbl_type = "";
            int lbl_ne = 0, lbl_s = 0;
            // 01 01 626F6F6C65616E 00 00
            // 01 02 68656C6C6F 00 2248656C6C6F20576F726C642122 00
            // 01 03 6E756D 00 000E 00
            // 01 04 636F6C 00 FFFFFF 00

            int lbl_ns = code_pos + 2;
            int lbl_e = code.ToList().IndexOf(0, code_pos) + 1;

            switch (code[code_pos + 1]) {
                case 1: 
                    lbl_ne = lbl_e - 1;
                    lbl_s = lbl_ne + 1;
                    if (code[lbl_s] == 2) lbl_content = true;
                    else lbl_content = false;
                    lbl_type = "boolean";
                    break;

                case 2:
                    lbl_ne = code.ToList().IndexOf(34, code_pos) - 1;
                    lbl_s = lbl_ne + 2;
                    lbl_e = code.ToList().IndexOf(0, lbl_s) - 1;
                    lbl_content = baToStr(code[lbl_s..lbl_e]);
                    lbl_type = "string";
                    break;

                case 3:
                    lbl_ne = lbl_e - 1;
                    lbl_e = lbl_e + 2;
                    lbl_s = lbl_ne + 1;
                    lbl_content = baToInt(code[lbl_s..lbl_e]);
                    lbl_e -= 1;
                    lbl_type = "number";
                    break;

                case 4:
                    lbl_ne = lbl_e - 1;
                    lbl_s = lbl_e;
                    lbl_e = lbl_e + 3;
                    lbl_type = "color";
                    lbl_content = baToCol(code[lbl_s..lbl_e]);
                    lbl_e -= 1;
                    break;
            }

            string lbl_name = baToStr(code[lbl_ns..lbl_ne]);

            global.addVar(lbl_name, lbl_type, lbl_content);
            code_pos = lbl_e + 2;
            return code_pos;
        }

        private int MTH(int code_pos, byte[] code) {
            // 02 01 6E756D31 00 03 000A 00
            // 02 02 6E756D31 00 01 6E756D31 00
            // 02 10 6E756D31 00 03 000A 00

            byte op = code[code_pos + 1];
            int lbl_e = code.ToList().IndexOf(0, code_pos);
            string lbl = baToStr(code[(2 + code_pos)..lbl_e]);
            dynamic lblc = global.getVar(lbl);

            byte numt = code[lbl_e + 1];
            int num_e = code.ToList().IndexOf(0, lbl_e + 1) + 2;
            dynamic numc = "";

            switch (numt) {
                case 2:
                    num_e -= 2;
                    numc = baToStr(code[(lbl_e + 3)..(num_e - 1)]);
                    break;
                case 3: numc = baToInt(code[(lbl_e + 2)..num_e]); break;
                case 4: numc = code[(lbl_e + 2)..num_e]; break;
            }

            switch (op) {
                case 1: lblc[1] += numc; break;
                case 2: lblc[1] -= numc; break;
                case 3: lblc[1] *= numc; break;
                case 4: lblc[1] /= numc; break;
                case 5: lblc[1] = numc; break;
                case 10: lblc = new dynamic[] { "boolean", lblc > numc }; break;
                case 11: lblc = new dynamic[] { "boolean", lblc < numc }; break;
                case 12: lblc = new dynamic[] { "boolean", lblc >= numc }; break;
                case 13: lblc = new dynamic[] { "boolean", lblc <= numc }; break;
                case 14: lblc = new dynamic[] { "boolean", lblc != numc }; break;
                case 15: lblc = new dynamic[] { "boolean", lblc == numc }; break;
            }

            global.setVar(lbl, lblc[0], lblc[1]);
            code_pos = num_e + 1;
            return code_pos;
        }

        private int PUT(int code_pos, byte[] code) {
            // 03 06 68656C6C6F0A 00
            // 03 02 2248656C6C6F20576F726C642122 00
            // 03 03 0001 00 
            // 03 01 01 00

            byte type = code[code_pos + 1];
            int end = code.ToList().IndexOf(0, code_pos + 3);
            object con = "";

            code_pos += 2;
            switch (type) {
                case 1: case 4: con = baToStr(code[code_pos..end]); break;
                case 2: con = baToStr(code[(code_pos + 1)..(end - 1)]); break;
                case 3: con = baToInt(code[code_pos..end]) + "\n"; break;
                case 6: con = global.getVar(baToStr(code[code_pos..end]))[1]; break;
            }

            Console.Write(con.ToString());

            code_pos = end + 1;
            return code_pos;
        }

        private int RKI(int code_pos, byte[] code) {
            // 04 796F75726E616D650A 00
            int end = code.ToList().IndexOf(0, code_pos + 3);
            string name = baToStr(code[(code_pos + 1)..end]);
            string? con = Console.ReadLine();

            if (global.varExists(name)) global.setVar(name, "string", con);
            else global.addVar(name, "string", con);

            code_pos = end + 1;
            return code_pos;
        }

        private int RET(int code_pos, byte[] code) {
            // 05 03 0000 0
            byte type = code[code_pos + 1];
            int end = code.ToList().IndexOf(0, code_pos + 3);
            int con = 0;

            code_pos += 2;
            switch (type) {
                case 3: con = baToInt(code[code_pos..end]); break;
                case 6: con = global.getVar(baToStr(code[code_pos..end]))[1]; break;
            }

            global.exit_code = con;
            return -1;
        }

        private int MRK(int code_pos, byte[] code) {
            // 06 6D61726B65723031 00
            int end = code.ToList().IndexOf(0, code_pos);
            string marker = baToStr(code[(code_pos + 1)..end]);
            int pos = end + 1;

            if (!global.markExists(marker)) global.addMark(marker, pos);
            else global.setMark(marker, pos);

            return pos;
        }

        private int JMP(int code_pos, byte[] code) {
            // 06 6D61726B65723031 00
            int end = code.ToList().IndexOf(0, code_pos);
            string marker = baToStr(code[(code_pos + 1)..end]);

            int pos = global.getMark(marker);
            if (global.last_jump.ContainsKey(marker))
                global.last_jump[marker] = pos;
            else global.last_jump.Add(marker, pos);

            return pos;
        }

        private int SSC(int code_pos, byte[] code) {
            // 08 03 0320 00 03 0258 00
            byte num1t = code[code_pos + 1];
            dynamic num1 = "", num2 = "";
            int num1e = code.ToList().IndexOf(0, code_pos);
            int num2e = code.ToList().IndexOf(0, num1e + 1);
            byte num2t = code[num1e + 1];

            code_pos += 2;
            switch (num1t) {
                case 3: num1 = baToInt(code[code_pos..num1e]); break;
                case 6: num1 = global.getVar(baToStr(code[code_pos..num1e])); break;
            }

            switch (num2t) {
                case 3: num2 = baToInt(code[(num1e + 1)..num2e]); break;
                case 6: num2 = global.getVar(baToStr(code[(num1e + 1)..num2e])); break;
            }

            if (global.mode == 0) {
                global.width = num1;
                global.height = num2;
                global.change_sr = true;
            }

            return code_pos;
        }

        private int BEP(int code_pos, byte[] code) {
            byte num1t = code[code_pos + 1];
            dynamic num1 = "", num2 = "";
            int num1e = code.ToList().IndexOf(0, code_pos);
            int num2e = code.ToList().IndexOf(0, num1e + 1);
            byte num2t = code[num1e + 1];

            code_pos += 2;
            switch (num1t) {
                case 3: num1 = baToInt(code[code_pos..num1e]); break;
                case 6: num1 = global.getVar(baToStr(code[code_pos..num1e])); break;
            }

            switch (num2t) {
                case 3: num2 = baToInt(code[(num1e + 1)..num2e]); break;
                case 6: num2 = global.getVar(baToStr(code[(num1e + 1)..num2e])); break;
            }

            Console.Beep(num1, num2);

            return code_pos;
        }

        private int IFJ(int code_pos, byte[] code) {
            return code_pos;
        }

        private int DTB(int code_pos, byte[] code) {
            // 0B 03 021C 00 03 00FA 00 04 0495AB 00
            dynamic num1 = "", num2 = "", col = "";
            int num1e = code.ToList().IndexOf(0, code_pos);
            int num2e = code.ToList().IndexOf(0, num1e + 1);
            int cole = code.ToList().IndexOf(0, num2e + 1);
            byte num1t = code[code_pos + 1];
            byte num2t = code[num1e + 1];
            byte colt = code[num2e + 1];

            code_pos += 2;
            switch (num1t) {
                case 3: num1 = baToInt(code[code_pos..num1e]); break;
                case 6: num1 = global.getVar(baToStr(code[code_pos..num1e])); break;
            }

            switch (num2t) {
                case 3: num2 = baToInt(code[(num1e + 1)..num2e]); break;
                case 6: num2 = global.getVar(baToStr(code[(num1e + 2)..num2e])); break;
            }

            switch (colt) {
                case 4: col = baToCol(code[(num2e + 1)..cole]); break;
                case 6: col = global.getVar(baToStr(code[(num2e + 2)..cole])); break;
            }
            
            RectangleShape pixel = new RectangleShape {
                Size = new Vector2f(1.0f, 1.0f),
                FillColor = new Color((uint)col[1]),
                Position = new Vector2f(num1[1],num2[1])
            };
            global.Buffer.Add(pixel);
            return cole + 1;
        }

        private int CDB(int code_pos, byte[] code) {
            return code_pos;
        }

        private int RFB(int code_pos, byte[] code) {
            return code_pos;
        }

        private int WFT(int code_pos, byte[] code) {
            // 0E 03 03E8 00
            byte type = code[code_pos + 1];
            int end = code.ToList().IndexOf(0, code_pos + 3);
            dynamic con = "";

            code_pos += 2;
            switch (type) {
                case 1: case 4: con = baToStr(code[code_pos..end]); break;
                case 2: con = baToStr(code[(code_pos + 1)..(end - 1)]); break;
                case 3: con = baToInt(code[code_pos..end]); break;
                case 6: con = global.getVar(baToStr(code[code_pos..end]))[1]; break;
            }

            Thread.Sleep(con);

            return end + 1;
        }

        private int IMP(int code_pos, byte[] code) {
            byte mode = code[code_pos + 1];

            if (mode == 2) codes = imp_opc;
            else if (mode == 1) codes = normal_opc;

            return code_pos + 2;

        }

        private int RTJ(int code_pos, byte[] code) {
            return code_pos;
        }
    }

    class GlobalData {
        public byte mode = 0;
        public int exit_code = -1;
        public uint width = 0, height = 0;
        public bool change_sr = false;

        public List<RectangleShape> Buffer;

        Dictionary<string, dynamic> vars = new Dictionary<string, dynamic>();
        Dictionary<string, int> markers = new Dictionary<string, int>();
        public Dictionary<string, int> last_jump = new Dictionary<string, int>();

        // Variable Modification -----------------------------------------------
        public void addVar(string name, string type, dynamic data) {
            vars.Add(name, new dynamic[] { type, data });
        }

        public dynamic getVar(string name) { return vars[name]; }

        public void setVar(string name, string type, dynamic data) {
            vars[name] = new dynamic[] { type, data };
        }

        public void removeVar(string name) { vars.Remove(name); }

        public bool varExists(string name) { return vars.ContainsKey(name); }

        // Marker Modification -------------------------------------------------
        public void addMark(string name, int position) { markers.Add(name, position); }

        public int getMark(string name) { return markers[name]; }

        public void setMark(string name, int position) { markers[name] = position; }

        public void removeMark(string name) { markers.Remove(name); }

        public bool markExists(string name) { return markers.ContainsKey(name); }
    }

    class Emulator {
        public byte[] content = [];
        private static GlobalData globdata = new GlobalData();
        private OperationCodes opcodes = new OperationCodes(globdata);
        private bool running = true;
        public int code_pos = 0;

        private RenderWindow window = new(new VideoMode(1, 1), "Emulator");

        public void PrintA(string type, byte[] bytes) {
            var sb = new StringBuilder(type + " as byte { ");
            int index = 0;
            foreach (var b in bytes) {
                sb.Append(b);
                if (bytes.Length - 1 > index) sb.Append(", ");
                index += 1;
            }
            sb.Append(" }");
            Console.WriteLine(sb.ToString());
            
        }

        // Conversions ----------------------------------------------
       public int baToInt(byte[] ba) {
            int result = 0;
            int multiplier = 1;

            for (int i = ba.Length - 1; i >= 0; i--) {
                result += ba[i] * multiplier;
                multiplier *= 256;
            }

            return result;
        }

        // Load BIN --------------------------------------------------
        public void loadBIN(string path) {
            content = File.ReadAllBytes(path);
        }


        // Prepare ---------------------------------------------------
        public void init() {
            window.Close();
            // Read out header
            byte[] header = content[0..16];
            List<byte> name = content[16..48].ToList();
            content = content[48..];

            globdata.mode = header[0];
            if (globdata.mode == 2) throw new Exception("LIBRARY_EXECUTED");

            globdata.width = (uint)baToInt(header[1..3]);
            globdata.height = (uint)baToInt(header[3..5]);

            opcodes = new OperationCodes(globdata);

            // Tidy up name

            name.RemoveAll(x => x == 0);
            String title = Encoding.Default.GetString(name.ToArray());

            // Initalize for each mode
            if (globdata.mode == 0) {
                VideoMode mode = new VideoMode(globdata.width, globdata.height, 32);
                window = new RenderWindow(mode, title, Styles.Default);
                window.SetActive(true);
                window.Closed += OnClose;

                globdata.Buffer = new List<RectangleShape>();
            } else if (globdata.mode == 1) {
                Console.Clear();

                Console.Title = title;
            }

            // Jump to the _start instruction if it exists
        }

        // Emulation ---------------------------------------------------
        void OnClose(object sender, EventArgs a) {
            window.Close();
            Environment.Exit(0);
            return;
        }

        public void loop() {
            code_pos = 0;

            while (running) {
                try {
                    int op_code = content[code_pos];
                    code_pos = (int)opcodes.codes[op_code].DynamicInvoke(code_pos, content);
                } catch (TargetInvocationException ex) { throw ex.InnerException; }

                if (globdata.mode == 0) {
                    // tBuf.Update([255,255,255,255],1,1,50,50);
                    if (globdata.change_sr) {
                        window.Size = new Vector2u(globdata.width, globdata.height);
                        globdata.change_sr = false;
                    }
                    
                    window.DispatchEvents();

                    window.Clear(Color.Black);
                    foreach (var pixel in globdata.Buffer) {
                        window.Draw(pixel);
                    }
                    window.Display();
                }

                if (code_pos == -1) {
                    Console.WriteLine("Program ended with code " + globdata.exit_code);
                    running = false;
                }
            }

        }
    }

    class EmulatorProgram {
        static Emulator emulator = new Emulator();

        public void Main(string[] args) {
            try {
                emulator.loadBIN(args[0]);
                emulator.init();
                emulator.loop();
                Console.ReadKey();
            } catch (Exception ex) {
                string pos = emulator.code_pos.ToString("X");
                int zlen = 8 - pos.Length;
                pos = new string('0', zlen) + pos;

                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Occured at 0x" + pos + " and at: " + ex.TargetSite);

                Console.ReadKey();
            }
        }
    }
}