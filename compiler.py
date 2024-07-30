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
ret_atend = False
ret_correct = False

header_exists = False
header_correct = False

# Translating the code
cur_line = 1
for line in content:
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

    elif line.lower().startswith("nop"): # 0x00 Inst
        inst.extend([0])

    elif ":" in line.lower(): # 0x01 Inst
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

        del lbl_name, lbl_con, lbl_content, lbl_type

    elif line.lower().startswith("add"): # 0x02 Inst
        con = line.split(" ")
        inst.extend([2]+str_to_ba(con[1])+[0]+str_to_ba(con[2])+[0])

        del con

    elif line.lower().startswith("put"): # 0x0A Inst
        con = line.split(" ")

        put_type = 1
        if "\"" in con[1]: put_type = 2
        if con[1].isnumeric(): put_type = 3

        put_content: list = str_to_ba(' '.join(con[1:]))
        if put_type == 3:
            put_content = num_to_ba(con[1])

        inst.extend([10,put_type]+put_content+[0])

        del put_type, put_content, con

    elif line.lower().startswith("ret"): # 0x0C Inst
        ret_exists = True
        con = line.split(" ")
        inst.extend([12,int(con[1])])
        ret_correct = True

        if cur_line == len(content): ret_atend = True

        del con

    elif line.lower().startswith(";"):
        pass

    else: raise Exception("COMPILER::USER_CODE::UNKNOWN_INST\nLine: "+str(cur_line))

    cur_line += 1

del cur_line, content, input_file

# Raising Exceptions if user forgot basic shit
if not ret_exists: raise Exception("COMPILER::USER_CODE::RETURN_MISSING")
if not ret_atend: raise Exception("COMPILER::USER_CODE::RETURN_NOT_AT_END")

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