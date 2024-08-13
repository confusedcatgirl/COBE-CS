import sys

# Conversion Functions
def num_to_ba(num: str):
    hnum = hex(int(num)).replace("0x","")
    rba = "0"*(4-len(hnum))+hnum
    ba = [rba[i:i + 2] for i in range(0, len(rba), 2)]
    iba = [int(i,16) for i in ba]
    return iba

def str_to_hex(s: str) -> str:
    return s.encode("utf-8").hex()

def str_to_ba(s):
    s = str_to_hex(s)
    return [int(s[i:i + 2],16) for i in range(0, len(s), 2)]

def op_to_by(op: str) -> int:
    if op == "+": return 1
    elif op == "-": return 2
    elif op == "*": return 3
    elif op == "/": return 4
    elif op == ">": return 10
    elif op == "<": return 11
    elif op == ">=": return 12
    elif op == "<=": return 13
    elif op == "!=": return 14
    elif op == "==": return 15
    else: return 0

def bool_to_by(bool: str) -> int:
    if bool == "true": return 2
    elif bool == "false": return 1
    else: return 0

input_file = open("test.asm","r").read()
content = input_file.split("\n")

header = [1,0,64,0,64,0,0,0,0,0,0,0,0,0,0,0]
inst = []

# Flags to check if code is proper
ret_exists = False
ret_correct = False

header_exists = False
header_correct = False

# Translating the code
cur_line = 1
for line in content:
    # TODO: REPLACE WITH SWITCH
    if line.lower().startswith("header:"): # Header
        header_exists = True
        head = line.split(" ")
        head[0] = head[0][7:].lower()

        if head[1] == "terminal": header[0] = 1
        elif head[1] == "graphical": header[0] = 0

        header[1:3] = num_to_ba(head[2])
        header[3:5] = num_to_ba(head[3])

        header_correct = True

        del head

    elif line.lower().startswith(";"): # Comment
        pass

    elif line.lower().startswith("nop"): # 0x00 Inst; NOP
        inst.extend([0])

    elif line.lower().startswith("lbl"): # 0x01 Inst; LBL String hello "Hello World!"
        con = line.split(" ")
        
        lbl_name = str_to_ba(con[2])

        lbl_type = ""
        if con[1].lower() == "number": lbl_type = 3
        elif con[1].lower() == "string": lbl_type = 2
        elif con[1].lower() == "boolean": lbl_type = 1
        else: raise Exception("COMPILER::USER_CODE::UNKNOWN_LABEL_TYPE")

        lbl_con = ' '.join(con[3:])
        lbl_content = str_to_ba(lbl_con)
        if lbl_type == 3: 
            lbl_content = num_to_ba(lbl_con)
            inst.extend([1,lbl_type]+lbl_name+[0, lbl_content[0], lbl_content[1]])
        else:
            inst.extend([1,lbl_type]+lbl_name+[0]+lbl_content)

        del lbl_name, lbl_con, lbl_content, lbl_type, con

    elif line.lower().startswith("mth"): # 0x02 Inst; MTH num1 + 10
        con = line.split(" ")
        if con[1].isdigit(): raise Exception("COMPILER::USER_CODE::LABEL_ARG_IS_NUMERIC\nLine: "+str(cur_line))

        num1 = str_to_ba(con[1])
        num2 = str_to_ba(con[3])
        num_type = 6

        if con[3].isdigit():
            num2 = num_to_ba(con[3])
            num_type = 3

        type = op_to_by(con[2])

        inst.extend([2,type]+num1+[0,num_type]+num2+[0])

        del con, num1, num2, type

    elif line.lower().startswith("put"): # 0x03 Inst; PUT hello; PUT "Hello World!"
        con = line.split(" ")

        type = 3
        if "\"" in con[1]: type = 2
        elif con[1] == "true" or con[1] == "false": type = 1
        elif con[1].startswith("#"): type = 4
        elif not con[1].isdigit(): type = 6

        put_content = 0
        if type == 2: put_content = str_to_ba(" ".join(con[1:])) # String
        elif type == 3: put_content = num_to_ba(con[1]) # Number
        elif type == 4: put_content = str_to_ba(con[1].replace("#","")) # Color
        elif type == 1 or type == 6: put_content = str_to_ba(con[1]) # Label / Boolean

        inst.extend([3,type]+put_content+[0])

        del con, type

    elif line.lower().startswith("rki"): # 0x04 Inst; RKI yourname
        con = line.split(" ")

        if len(con) > 2: raise Exception("COMPILER::USER_CODE::RKI_TOO_LONG\nLine: "+str(cur_line)+"\nFile: "+str(sys.argv[0]))
        if con[1].isdigit(): raise Exception("COMPILER::USER_CODE::LABEL_IS_NUMBER\nLine: "+str(cur_line)+"\nFile: "+str(sys.argv[0]))
        if con[1].startswith("\""): raise Exception("COMPILER::USER_CODE::LABEL_IS_STRING\nLine: "+str(cur_line)+"\nFile: "+str(sys.argv[0]))

        inst.extend([4]+str_to_ba(con[1])+[0])

        del con

    elif line.lower().startswith("ret"): # 0x05 Inst; RET 24; RET deez
        ret_exists = True
        con = line.split(" ")

        type = 3 if con[1].isdigit() else 6
        ret_code = num_to_ba(con[1]) if type == 3 else str_to_ba(con[1])

        inst.extend([5,type]+ret_code+[0])
        ret_correct = True

        del con, type

    elif line.lower().startswith("mrk"): # 0x06 Inst; MRK Slime
        con = line.split(" ")

        if len(con) > 2: raise Exception("COMPILER::USER_CODE::MRK_TOO_LONG\nLine: "+str(cur_line)+"\nFile: "+str(sys.argv[0]))
        if con[1].isdigit(): raise Exception("COMPILER::USER_CODE::MARKER_IS_NUMBER\nLine: "+str(cur_line)+"\nFile: "+str(sys.argv[0]))
        if con[1].startswith("\""): raise Exception("COMPILER::USER_CODE::MARKER_IS_STRING\nLine: "+str(cur_line)+"\nFile: "+str(sys.argv[0]))

        inst.extend([6]+str_to_ba(con[1])+[0])

        del con

    elif line.lower().startswith("jmp"): # 0x07 Inst; JMP Slime
        con = line.split(" ")

        if len(con) > 2: raise Exception("COMPILER::USER_CODE::JMP_TOO_LONG\nLine: "+str(cur_line)+"\nFile: "+str(sys.argv[0]))
        if con[1].isdigit(): raise Exception("COMPILER::USER_CODE::MARKER_IS_NUMBER\nLine: "+str(cur_line)+"\nFile: "+str(sys.argv[0]))
        if con[1].startswith("\""): raise Exception("COMPILER::USER_CODE::MARKER_IS_STRING\nLine: "+str(cur_line)+"\nFile: "+str(sys.argv[0]))

        inst.extend([7]+str_to_ba(con[1])+[0])

        del con

    elif line.lower().startswith("ssc"): # 0x08 Inst; SSC 800 800
        con = line.split(" ")

        num1t, num2t, num1, num2 = 0, 0, 0, 0

        if con[1].isdigit():
            num1t = 3
            num1 = num_to_ba(con[1])

        if con[2].isdigit():
            num2t = 3
            num2 = num_to_ba(con[2])

        inst.extend([8,num1t]+num1+[0,num2t]+num2+[0])

        del con, num1t, num2t, num1, num2

    elif line.lower().startswith("bep"): # 0x09 Inst; BEP 1000 1000
        con = line.split(" ")

        num1t, num2t, num1, num2 = 0, 0, 0, 0

        if con[1].isdigit():
            num1t = 3
            num1 = num_to_ba(con[1])

        if con[2].isdigit():
            num2t = 3
            num2 = num_to_ba(con[2])

        inst.extend([9,num1t]+num1+[0,num2t]+num2+[0])

        del con, num1t, num2t, num1, num2

    elif line.lower().startswith("ifj"): # 0x0A Inst; IFJ 10 >= num1 marker01
        con = line.split(" ")

        num1t, num2t, num1, num2, op, marker = 6, 6, str_to_ba(con[1]), str_to_ba(con[3]), op_to_by(con[2]), str_to_ba(con[4])

        if con[1].isdigit():
            num1t = 3
            num1 = num_to_ba(con[1])
        elif con[1] == "true" or con[1] == "false":
            num1t = 1
            num1 = bool_to_by(con[1])

        if con[3].isdigit():
            num2t = 3
            num2 = num_to_ba(con[3])
        elif con[3] == "true" or con[3] == "false":
            num2t = 1
            num2 = bool_to_by(con[3])

        inst.extend([10,op,num1t]+num1+[0,num2t]+num2+[0]+marker+[0])

        del con, num1t, num2t, num1, num2, op, marker

    elif line.lower().startswith("dtb"): # 0x0B Inst; DTB 300 250 #0495AB | 0B 03 021C 00 03 00FA 00 04 0495AB 00
        con = line.split(" ")

        num1t, num2t, num1, num2 = 0, 0, 0, 0

        type = 4 if con[1].startswith("#") else 6
        num = str_to_ba(con[2].replace("#","")) if type == 4 else str_to_ba(con[2])

        if con[1].isdigit():
            num1t = 3
            num1 = num_to_ba(con[1])

        if con[2].isdigit():
            num2t = 3
            num2 = num_to_ba(con[2])

        inst.extend([11,num1t]+num1+[0,num2t]+num2+[0,type]+num)

        del con, num1t, num2t, num1, num2, type, num

    elif line.lower().startswith("cdb"): # 0x0C Inst; CDB #000000
        con = line.split(" ")

        type = 4 if con[1].startswith("#") else 6
        num = str_to_ba(con[2].replace("#","")) if type == 4 else str_to_ba(con[2])

        inst.insert([12+type]+num+[0])

        del con, type, num

    elif line.lower().startswith("rfb"): # 0x0D Inst; RFB 301 250
        con = line.split(" ")

        num1t, num2t, num1, num2 = 0, 0, 0, 0

        if con[1].isdigit():
            num1t = 3
            num1 = num_to_ba(con[1])

        if con[2].isdigit():
            num2t = 3
            num2 = num_to_ba(con[2])

        inst.extend([13,num1t]+num1+[0,num2t]+num2+[0])

        del con, num1, num2, num1t, num2t

    elif line.lower().startswith("wft"): # 0x0F Inst; WFT 1000
        con = line.split(" ")

        type = 3 if con[1].isdigit() else 6
        num = num_to_ba(con[2]) if con[1].isdigit() else str_to_ba(con[2])

        inst.extend([14,type]+num+[0])

        del con, type, num

    elif str.isspace(line) or line == '':
        pass

    else: raise Exception("COMPILER::USER_CODE::UNKNOWN_INST\nLine: "+str(cur_line)+"\nFile: "+str(sys.argv[0]))

    cur_line += 1

del cur_line, content, input_file

# Raising Exceptions if user forgot basic shit
if not ret_exists: raise Exception("COMPILER::USER_CODE::RETURN_MISSING")
if not ret_correct: raise Exception("COMPILER::USER_CODE::RETURN_MALFORMED")

if not header_exists: raise Exception("COMPILER::USER_CODE::HEADER_MISSING")
if not header_correct: raise Exception("COMPILER::USER_CODE::HEADER_MALFORMED")

# Converting into binary
bin_content = bytearray()
bin_content.extend(header)
bin_content.extend(inst)

# Writing Binary to File
bin_file = open("test.bin","wb")
bin_file.write(bin_content)
bin_file.close()