Header: graphical 800 600
Title: DrawTest

MRK newline
MTH y + 1
MTH x = 0
RTJ newline

MRK _start

LBL Number x 1
LBL Number y 15
LBL Color col #FFFFFF

MRK loop
DTB x y col
MTH x + 1
JMP loop

RET 1