# Conversion Functions
def num_to_ba(num):
    hnum = hex(int(num)).replace("0x","")
    rba = "0"*(4-len(hnum))+hnum
    ba = [rba[i:i + 2] for i in range(0, len(rba), 2)]
    iba = [int(i,16) for i in ba]
    return iba

def str_to_hex(s):
    return s.encode("utf-8").hex()

def str_to_ba(s):
    s = str_to_hex(s)
    return [int(s[i:i + 2],16) for i in range(0, len(s), 2)]

def op_to_by(op):
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

input_file = open("test.asm","r").read()
content = []

# Simplifying the code and stripping empty lines
for line in input_file.split("\n"):
    if not str.isspace(line) and line != '':
        content.append(line)

header = [1,0,64,0,64,0,0,0,0,0,0,0,0,0,0,0]
labels = []
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
        head = line.replace(" ","").split(",")
        head[0] = head[0][7:].lower()

        if head[0] == "terminal": header[0] = 1
        elif head[0] == "graphical": header[0] = 0

        header[1:3] = num_to_ba(head[1])
        header[3:5] = num_to_ba(head[2])

        header_correct = True

        del head

    elif line.lower().startswith(";"): # Comment
        pass

    elif line.lower().startswith("nop"): # 0x00 Inst; NOP
        inst.extend([0])

    elif ":" in line.lower() and ";" not in line: # 0x01 Inst; String hello: "Hello World!" | 01 02 68656C6C6F 00 2248656C6C6F20576F726C642122 
        label = line.split(" ")
        
        lbl_name = str_to_ba(label[1].replace(":",""))

        lbl_type = ""
        if label[0].lower() == "number": lbl_type = 2
        elif label[0].lower() == "string": lbl_type = 1
        elif label[0].lower() == "boolean": lbl_type = 0
        else: raise Exception("COMPILER::USER_CODE::UNKNOWN_LABEL_TYPE")

        lbl_con = ''.join(label[2:])
        lbl_content = [lbl_con[i:i + 2] for i in range(0, len(lbl_con), 2)]
        if lbl_type == 2: 
            lbl_content = num_to_ba(lbl_con)
            labels.extend([1,lbl_type]+lbl_name+[0, lbl_content[0], lbl_content[1]])
        else:
            labels.extend([1,lbl_type]+lbl_name+[0, lbl_content])

        del lbl_name, lbl_con, lbl_content, lbl_type, label

    elif line.lower().startswith("mth"): # 0x02 Inst; MTH num1 + 10 | 02 06 6E756D31 00 00 000A 00
        con = line.split(" ")

        num1 = con[1]
        num2 = str_to_ba(con[3])
        num_type = 1
        if con[3].isnumeric():
            num2 = num_to_ba(int(con[3]))
            num_type = 0
        type = op_to_by(con[2])

        if num1.isnumeric(): raise Exception("COMPILER::USER_CODE::LABEL_ARG_IS_NUMERIC\nLine: "+str(cur_line))

        inst.extend([2,type]+str_to_ba(num1)+[0,num_type]+num2+[0])

        del con, num1, num2, type

    elif line.lower().startswith("put"): # 0x03 Inst
        con = line.split(" ")

        type = 3
        if "\"" in con[1]: type = 2
        elif not con[1].isnumeric(): type = 1

        put_content = 0
        if type == 1: put_content = str_to_ba(con[1])
        if type == 2: put_content = str_to_ba(" ".join(con[1:]))
        if type == 3: put_content = num_to_ba(con[1])

        inst.extend([3,type]+put_content+[0])

        del con, type

    elif line.lower().startswith("rki"): # 0x04 Inst
       con = line.split(" ")
       inst.extend([4]+str_to_ba(con[1])+[0])

       del con

    elif line.lower().startswith("ret"): # 0x05 Inst
        ret_exists = True
        con = line.split(" ")
        inst.extend([5,int(con[1])])
        ret_correct = True

        if cur_line == len(content): ret_atend = True

        del con

    elif line.lower().startswith("mrk"): # 0x06 Inst
        con = line.split(" ")
        inst.extend([6]+str_to_ba(con[1])+[0])

        del con

    elif line.lower().startswith("jmp"): # 0x07 Inst
        con = line.split(" ")
        inst.extend([7]+str_to_ba(con[1])+[0])

        del con

    elif line.lower().startswith("ssc"): # 0x08 Inst
        con = line.split(" ")
        inst.extend([8]+num_to_ba(int(con[1]))+num_to_ba(int(con[2])))

        del con

    elif line.lower().startswith("bep"): # 0x09 Inst
        inst.append(9)

    elif line.lower().startswith("ifj"): # 0x0A Inst
        con = line.split(" ")

        con[3] = "".join(con[3].rsplit(":", len(con[3])))

        arg1t = int(not con[1].isnumeric())
        arg2t = int(not con[3].isnumeric())
        arg1 = num_to_ba(int(con[1])) if arg1t == 1 else str_to_ba(con[1])
        arg2 = num_to_ba(int(con[3])) if arg2t == 1 else str_to_ba(con[3])
        op = op_to_by(con[2])

        inst.extend([10])

        del con, arg1, arg2, arg1t, arg2t, op

        pass

    else: raise Exception("COMPILER::USER_CODE::UNKNOWN_INST\nLine: "+str(cur_line)+"\nFile: ")

    cur_line += 1

del cur_line, content, input_file

'''
| 0x0A | IFJ | When the If is true, it will go down, otherwise jump to the marker. | IFJ 10 >= num1 marker01 | 0A 00000A 00 12 016E756D31 00 6D61726B65723031 00 |
| | | | IFJ num1 < 19 marker1 | 0A 016E756D31 11 000013 00 6D61726B65723031 00 |
| 0x0B | DTB | Draws a pixel to the buffer at a specific position. | DTB 300 250 #0495AB | 0B 021C 00FA 0495AB |
| 0x0C | CDB | Clears the draw buffer. | CDB | 0C |
| 0x0D | RFB | Removes a pixel from the draw buffer. | RFB 301 250 | 0D 021D 00FA |
| 0x0E | WFT | Waits a specific time in milliseconds. | WFT 1000 | 0E 03E8 |
| 0x0F | LFL | Loads a specific file into a label. | LFL text N "test.txt" | 0F 74657874 00 01 22746573742E74787422 00 |
| | | | LFL bin B "binary.bin" | 0F x 00 02 2262696E6172792E62696E22 |

'''

# Raising Exceptions if user forgot basic shit
if not ret_exists: raise Exception("COMPILER::USER_CODE::RETURN_MISSING")
if not ret_correct: raise Exception("COMPILER::USER_CODE::RETURN_MALFORMED")

if not header_exists: raise Exception("COMPILER::USER_CODE::HEADER_MISSING")
if not header_correct: raise Exception("COMPILER::USER_CODE::HEADER_MALFORMED")

# Converting into binary
bin_content = bytearray()
bin_content.extend(header)
bin_content.extend(labels)
bin_content.extend(inst)

# Writing Binary to File
bin_file = open("test.bin","wb")
bin_file.write(bin_content)
bin_file.close()