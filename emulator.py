import sys, pygame, struct, os

class OperationCodes:
    def __init__(self, globdata):
        self.codes = [self.NOP,self.LBL,self.ADD,self.ADV,self.SUB,
                      self.SBV,self.MUL,self.MUV,self.DIV,self.DVV,
                      self.PUT,self.NFC,self.RET,self.NFC,self.NFC,
                      self.NFC,self.NFC,self.NFC,self.NFC,self.NFC,
                      self.NFC,self.NFC,self.NFC,self.NFC,self.NFC,
                      self.NFC,self.NFC,self.NFC,self.NFC,self.NFC]
        self.global_data = globdata

    def NOP(self,code_pos,code): # 0x00
        code_pos += 1
        return code_pos

    def LBL(self,code_pos,code): # 0x01
        lbl_type, lbl_name, lbl_content, lbl_n_start, lbl_n_end, lbl_start, lbl_end = 0,0,0,0,0,0,0

        # Get Type
        lbl_type = code[code_pos+1]
        if lbl_type == "0": lbl_type = "bool"
        elif lbl_type == "1": lbl_type = "string"
        elif lbl_type == "2": lbl_type = "number"

        # Get Areas of Label
        lbl_n_start = code_pos + 2
        lbl_end = code.index("0",code_pos + 2)
        if lbl_type == "bool":
            lbl_end = lbl_end + 2
            lbl_n_end = lbl_end - 2
        elif lbl_type == "number":
            lbl_end = lbl_end + 3
            lbl_n_end = lbl_end - 3
        elif lbl_type == "string":
            lbl_n_end = code.index("22",code_pos + 2) 
            lbl_end = code.index("0",code_pos + 2) - 1
        lbl_start = lbl_n_end + 1

        # Get Name & Content
        lbl_name = ''.join(code[lbl_n_start:lbl_n_end])
        lbl_name = bytearray.fromhex(lbl_name).decode()

        if lbl_type == "string":
            lbl_content = ''.join(code[lbl_start+1:lbl_end])
            lbl_content = bytearray.fromhex(lbl_content).decode()
        elif lbl_type == "bool":
            if code[lbl_start] == "0": lbl_content = False
            else: lbl_content = True
        elif lbl_type == "number":
            lbl_content = ba2_to_int(code[lbl_start:lbl_end+1])

        self.global_data.add_var(lbl_name,lbl_type,lbl_content)
        code_pos += lbl_end - lbl_n_start + 2

        return code_pos
    
    def ADD(self,code_pos,code): # 0x02
        var1ind = code_pos + 1
        var2ind = code.index("0",var1ind + 1)
        var2end = code.index("0",var2ind + 1)

        label1 = ba_to_string(code[var1ind:var2ind])
        label2 = ba_to_string(code[var2ind+1:var2end])

        var1 = self.global_data.get_var(label1)
        var2 = self.global_data.get_var(label2)

        self.global_data.set_var(label1,var1[0],var1[1]+var2[1])

        code_pos += (var2end - code_pos) 
        return code_pos

    def ADV(self,code_pos,code): # 0x03
        var_ind = code_pos + 1
        var_end = code.index("0",var_ind + 1)

        label = ba_to_string(code[var_ind:var_end])
        var = self.global_data.get_var(label)[1]

        num = ba2_to_int(code[var_end+1:var_end+3])
        self.global_data.set_var(label,'number',var+num)
    
        code_pos = var_end+3
        return code_pos

    def SUB(self,code_pos,code): # 0x04
        code_pos += 3
        return code_pos

    def SBV(self,code_pos,code): # 0x05
        code_pos += 3
        return code_pos

    def MUL(self,code_pos,code): # 0x06
        code_pos += 3
        return code_pos

    def MUV(self,code_pos,code): # 0x07
        code_pos += 3
        return code_pos

    def DIV(self,code_pos,code): # 0x08
        code_pos += 3
        return code_pos

    def DVV(self,code_pos,code): # 0x09
        code_pos += 3
        return code_pos
    
    def PUT(self,code_pos,code): # 0x0A
        end_ind = code_pos + 4
        if code[end_ind] != "0": end_ind = code.index("0", code_pos + 3)
        type = code[code_pos + 1]

        if type == "3":
            print(ba2_to_int(code[code_pos+2:end_ind]))
        elif type == "2":
            print(ba_to_string(code[code_pos+3:end_ind-1]))
        elif type == "1":
            label = ba_to_string(code[code_pos+2:end_ind])
            var = self.global_data.get_var(label)
            print(var[1])


        code_pos = end_ind
        return code_pos                                                     
    
    def RET(self,code_pos,code): # 0x14
        self.global_data.exit_code = int(code[code_pos + 1],16)
        return -1

    def NFC(self,code_pos,code): # Exception Protection
        print("Unknown code "+code[code_pos]+"!")
        code_pos += 1
        return code_pos

class GlobalData:
    def __init__(self):
        self.mode = 0
        self.screen_size = [0,0]
        self.variables = {}
        self.exit_code = 0

    def add_var(self,name,type,data):
        self.variables.update({name:[type,data]})

    def get_var(self,name: str):
        return self.variables[name]
    
    def set_var(self,name,type,data):
        self.variables.update({name:[type,data]})

def ba2_to_int(list):
    num1 = list[0]
    num2 = list[1]

    nums = [-1,-1,-1,-1]

    # Assign numbers to list ('3', '32') - > [0,3,2,0]
    if len(num1) == 1:
        nums[0] = 0
        nums[1] = int(num1)
    else:
        nums[0] = int(num1[0],16)
        nums[1] = int(num1[1],16)

    if len(num2) == 1:
        nums[2] = 0
        nums[3] = int(num2)
    else:
        nums[2] = int(num2[0],16)
        nums[3] = int(num2[1],16)
    
    return nums[0]*(16**3) + nums[1]*(16**2) + nums[2]*16 + nums[3]

def ba_to_string(list):
    s = ""
    for i in list:
        s += str(i)

    return bytearray.fromhex(s).decode()

def init(header,global_data):
    mode = int(header[0])
    screen_width = ba2_to_int(header[1:3])
    screen_height = ba2_to_int(header[3:5])

    global_data.screen_size = [screen_height,screen_width]
    global_data.mode = mode

    if mode == 0: # Graphics Mode
        pygame.init()
        pygame.display.set_mode((screen_width,screen_height))
    if mode == 1: # Terminal Mode
        os.system("cls || clear")

def read_file():
    raw_content = open(sys.argv[1],"rb").read()
    content = struct.unpack("B" * len(raw_content),raw_content)

    formatted = []
    for byte in content:
        formatted.append(hex(byte).split('x')[-1])

    return formatted

try:
    if len(sys.argv) < 2: raise Exception("Missing arguments! No Input file!")

    content = read_file()
    code_pos = 0
    running = True

    global_data = GlobalData()
    op_codes = OperationCodes(global_data)

    init(content[0:16],global_data)
    content = content[16:]
    while running:
        op_code = int(content[code_pos],16)
        code_pos = op_codes.codes[op_code](code_pos,content)

        # Displaying things
        if global_data.mode == 0:
            for event in pygame.event.get():
                if event.type == pygame.QUIT: running = False

            pygame.display.flip()

        if code_pos == -1:
            running = False

    print("The app returned code "+str(global_data.exit_code)+".")

except Exception as ex:
    pygame.quit()
    print(str(ex))
    input("Press ENTER to Exit. ")