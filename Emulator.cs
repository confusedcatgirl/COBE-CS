
using System.Text;
using SFML.Window;
using SFML.Graphics;
using SFML.System;
using System.Globalization;

#pragma warning disable CS8622 // Nullability of reference types
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CS8604 // Possible null reference argument.

namespace COBE_CS {
    class OperationCodes {
        private GlobalData global;
        public List<Delegate> codes = new List<Delegate>();
        // https://stackoverflow.com/questions/42159436/store-methods-in-an-array-and-call-them-in-c-sharp
        
        public OperationCodes(GlobalData globdata) {
            codes.AddRange([NOP, LBL, MTH, PUT, RKI, RET, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                            NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                            NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                            NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                            NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                            NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP,
                            NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP]);
            global = globdata;
        }

        public string baToStr(byte[] encstr) {
            return Encoding.Default.GetString(encstr);
        }

        public int baToInt(byte[] ba) {
            // [03, 20] -> 800

            int num = 0;
            byte pos = 0;
            for (int i=1; i >= 0; i--) { num += ba[pos]*(int)Math.Pow(16,i); pos++; }
            // [0, 5] -> ba[pos=0] = 0; 16^i=1 = 16; num += 0 * 256
            //           ba[pos=1] = 5; 16^i=1 = 0; num += 5 * 1
            // [0, 5] -> num = 5

            return num;
        }

        public uint baToCol(byte[] col) {
            byte r = col[0], g = col[1], b = col[2];
            return (uint)((r << 16) | (g << 8) | (b << 0));
        }


        private int NOP(int code_pos, byte[] code) {
            Console.WriteLine("NOP: "+code[code_pos]);
            code_pos += 1;
            return code_pos;
        }

        private int LBL(int code_pos, byte[] code) {
            dynamic lbl_content = ""; string lbl_type = "";
            int lbl_ne = 0, lbl_s = 0;
            // 01 01 626F6F6C65616E 00 00
            // 01 02 68656C6C6F 00 2248656C6C6F20576F726C642122
            // 01 03 6E756D 00 000E

            if (code[code_pos+1] == 1) lbl_type = "boolean";
            else if (code[code_pos+1] == 2) lbl_type = "string";
            else if (code[code_pos+1] == 3) lbl_type = "number";

            int lbl_ns = code_pos + 2;
            int lbl_e = code.ToList().IndexOf(0,code_pos) + 1;

            if (lbl_type == "boolean") {
                lbl_ne = lbl_e - 1;
                lbl_s = lbl_ne + 1;
                if (code[lbl_s] == 2) lbl_content = true;
                else lbl_content = false;
            } else if (lbl_type == "number") {
                lbl_ne = lbl_e - 1;
                lbl_e = lbl_e + 3;
                lbl_s = lbl_ne + 1;
                lbl_content = baToInt(code[lbl_s..lbl_e]);
                lbl_e -= 2;
            } else if (lbl_type == "string") {
                lbl_ne = code.ToList().IndexOf(34,code_pos) - 1;
                lbl_s = lbl_ne + 2;
                lbl_e = code.ToList().IndexOf(0,lbl_s) - 1;
                lbl_content = baToStr(code[lbl_s..lbl_e]);
            }

            string lbl_name = baToStr(code[lbl_ns..lbl_ne]);

            global.addVar(lbl_name,lbl_type,lbl_content);            
            code_pos = lbl_e + 2;
            return code_pos;
        }

        private int MTH(int code_pos, byte[] code) {
            // 02 01 6E756D31 00 03 000A 00
            // 02 02 6E756D31 00 01 6E756D31 00
            // 02 10 6E756D31 00 03 000A 00

            byte op = code[code_pos + 1];
            int lbl_e = code.ToList().IndexOf(0,code_pos);
            string lbl = baToStr(code[(2 + code_pos)..lbl_e]);
            dynamic lblc = global.getVar(lbl);

            byte numt = code[lbl_e + 1];
            int num_e = code.ToList().IndexOf(0,lbl_e + 1) + 2;
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
                case 10: lblc = new dynamic[]{ "boolean", lblc > numc }; break;
                case 11: lblc = new dynamic[]{ "boolean", lblc < numc }; break;
                case 12: lblc = new dynamic[]{ "boolean", lblc >= numc }; break;
                case 13: lblc = new dynamic[]{ "boolean", lblc <= numc }; break;
                case 14: lblc = new dynamic[]{ "boolean", lblc != numc }; break;
                case 15: lblc = new dynamic[]{ "boolean", lblc == numc }; break;
            }

            global.setVar(lbl,lblc[0],lblc[1]);
            code_pos = num_e + 1;
            return code_pos;
        }

        private int PUT(int code_pos, byte[] code) {
            // 03 06 68656C6C6F0A 00
            // 03 02 2248656C6C6F20576F726C642122 00
            // 03 03 0001 00 
            // 03 01 01 00

            byte type = code[code_pos + 1];
            int end = code.ToList().IndexOf(0,code_pos + 3);
            object con = "";

            code_pos += 2;
            switch (type) {
                case 1: case 4: con = baToStr(code[code_pos..end]); break;
                case 2: con = baToStr(code[(code_pos+1)..(end-1)]); break;
                case 3: con = baToInt(code[code_pos..end]) + "\n"; break;
                case 6: con = global.getVar(baToStr(code[code_pos..end]))[1]; break;
            }

            Console.Write(con.ToString());

            code_pos = end + 1;
            return code_pos;
        }

        private int RKI(int code_pos, byte[] code) {
            // 04 796F75726E616D650A 00
            int end = code.ToList().IndexOf(0,code_pos + 3);
            string name = baToStr(code[(code_pos + 1)..end]);
            string? con = Console.ReadLine();

            if (global.varExists(name)) global.setVar(name,"string",con);
            else global.addVar(name,"string",con);

            code_pos = end + 1;
            return code_pos;
        }

        private int RET(int code_pos, byte[] code) {
            // 05 03 0000 00
            byte type = code[code_pos + 1];
            int end = 0;
            int con = 0;

            code_pos += 2;
            switch (type) {
                case 3: con = baToInt(code[code_pos..end]); break;
                case 6: con = global.getVar(baToStr(code[code_pos..end]))[1]; break;
            }

            global.exit_code = con;
            return code_pos;
        }
    }

    class GlobalData {
        public byte mode = 0;
        public int exit_code = -1;
        public uint width = 0, height = 0;

        Dictionary<string, dynamic> var = new Dictionary<string, dynamic>(); 
        Dictionary<string, int> markers = new Dictionary<string, int>();
        Dictionary<string, int> last_jump = new Dictionary<string, int>();

        // Variable Modification -----------------------------------------------
        public void addVar(string name, string type, dynamic data) {
            var.Add(name,new dynamic[]{ type, data });
        }

        public dynamic getVar(string name) {
            return var[name];
        }

        public void setVar(string name, string type, dynamic data) {
            var[name] = new dynamic[]{ type, data };
        }

        public void removeVar(string name) { var.Remove(name); }

        public bool varExists(string name) {
            return var.ContainsKey(name);
        }

        // Marker Modification -------------------------------------------------
        public void addMark(string name, int position) {
            markers.Add(name,position);
        }

        public int getMark(string name) { return markers[name]; }

        public void setMark(string name, int position) { markers[name] = position; }

        public void removeMark(string name) { markers.Remove(name); }
    }

    class Emulator {
        public byte[] content = new byte[0];
        private GlobalData globdata = new GlobalData();
        private OperationCodes opcodes;
        private bool running = true;
        public int code_pos = 0;

        private RenderWindow window;

        private Texture tBuf = new Texture(1,1);
        private Sprite Buffer = new Sprite();

        public void PrintA(string type, byte[] bytes) {
            var sb = new StringBuilder(type+" as byte { ");
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
            // [03, 20] -> 800

            int num = 0;
            byte pos = 0;
            for (int i=1; i >= 0; i--) { num += ba[pos]*(int)Math.Pow(16,i); pos++; }
            // [0, 5] -> ba[pos=0] = 0; 16^i=1 = 16; num += 0 * 256
            //           ba[pos=1] = 5; 16^i=1 = 0; num += 5 * 1
            // [0, 5] -> num = 5

            return num;
        }

        
        // Load BIN --------------------------------------------------
        public void loadBIN(string path) {
            content = File.ReadAllBytes(path);
        }


        // Prepare ---------------------------------------------------
        public void init() {
            byte[] header = content[0..16];
            content = content[16..];

            globdata.mode = header[0];
            if (globdata.mode == 2) throw new Exception("LIBRARY_EXECUTED");
            globdata.width = (uint)baToInt(header[1..3]);
            globdata.height = (uint)baToInt(header[3..5]);

            opcodes = new OperationCodes(globdata);

            if (globdata.mode == 0) {
                VideoMode mode = new VideoMode(globdata.width,globdata.height,32);
                window = new RenderWindow(mode, "COBE Emulator",Styles.Default);
                window.SetActive(true);
                window.Closed += OnClose;

                tBuf = new Texture(globdata.width,globdata.height);
                Buffer = new Sprite(tBuf);
            } else if (globdata.mode == 1) {
                Console.Clear();

                Console.SetWindowSize((int)globdata.width,(int)globdata.height/4);
                Console.SetBufferSize((int)globdata.width,(int)globdata.height*16);
            }
        }

        // Emulation ---------------------------------------------------
        void OnClose(object sender, EventArgs a) {
            window.Close();
        }

        public void loop() {
            code_pos = content.ToList().LastIndexOf(255) + 1;

            while (running) {
                int op_code = content[code_pos];
                Console.WriteLine(code_pos+": "+op_code);
                code_pos = (int)opcodes.codes[op_code].DynamicInvoke(code_pos, content);

                if (globdata.mode == 0) {
                    // tBuf.Update([255,255,255,255],1,1,50,50);

                    window.DispatchEvents();

                    window.Draw(Buffer);
                    window.Display();
                }

                if (globdata.exit_code != -1) {
                    Console.WriteLine("Program ended with code "+globdata.exit_code);
                    running = false;
                }
            }

        }
    }

    class EmulatorProgram {
        static Emulator emulator = new Emulator();

        public void Main(string[] args) {
            //try {
                emulator.loadBIN(args[0]);
                emulator.init();
                emulator.loop();
                Console.ReadLine();
            /*} catch (Exception ex) {
                string pos = emulator.code_pos.ToString("X");
                int zlen = 8-pos.Length;
                pos = new string('0', zlen) + pos;

                Console.WriteLine("Error: "+ex.Message);
                Console.WriteLine("Occured at 0x"+pos+" and at: "+ex.Source);

                Console.ReadLine();
            }*/
        }
    }
}