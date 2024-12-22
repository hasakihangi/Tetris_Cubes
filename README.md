# 操作方式 operation
A, D: 左右移动

Q，E：左右旋转

S：加速下落

Space：确定下落

# Features
## 音效系统 sound effect system
基于对象池的音效系统

## 存储系统 save/load system
二进制存储

![tetris_save_2](https://github.com/user-attachments/assets/41f768cd-da4e-4f97-8ec6-6f2bbc27b065)


## timeline系统 timeline system
通过timeline安排演出动画

![tetris_timeline](https://github.com/user-attachments/assets/928fb3ee-afbf-4fed-9b28-540cb0b6ec72)

架构来自于猴叔(猴与花果山 kierstone), 改了部分: 

timeline中的list换成linkedList

timeline和timelineNode获取全部通过对象池

timelineManager中遍历timeline的结构换成了dictionary
