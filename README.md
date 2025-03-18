# For, While and Until loops for VNyan
Adds triggers to VNyan to do loops without having to manually implement them in a node graph.  

![VNyan-Loops](https://github.com/user-attachments/assets/376f7ced-65c0-40d8-aa8f-3248d020f4bf)


## Installation
Download the attached zip file and unpack to VNyan\Items\Assemblies

## For
```_lum_loop_for``` - Loop from one number to another  
text 1 (optional) - Name of the decimal to store the loop index into  
text 2 - Trigger to call every time we loop  
text 3 - Trigger to call once we exit the loop  
number 1 - Start value. We will start looping from this number  
number 2 - Target value. We will finish looping at this number  
number 3 (optional) - Increment - we will add this value to the counter every loop (defaults to either 1 or -1 if unspecified or an invalid value is given)  

While the loop index is stored into the specified decimal it is NOT read. If you need to mess with the index at runtime, use a while loop instead.

### Callback 
number 1 - The current loop index  
number 2 - Number of times we have looped (-1 if done)  
number 3 - Session ID (set this with e.g. ```_lum_loop_for;sessionid=69```)  

This is rougly equivalent to doing the following in (Visual) Basic
```
For N = num1 to num2 Step num3
  SetVNyanParam(text1,N)
  CallVNyan(text2,N)
Next N
CallVNyan(text3,N)
```
## ForEach
The Loops-ForEach-Support.json nodegraph must be imported for these functions to work. Unfortunately VNyan plugins cannot directly access (T)Arrays, so this graph packages them up in a format we can access. Unfortunately because of this SessionID and delay parameters cannot be set in the trigger name with a ;

```_lum_loop_foreach``` - Loop through an array  
text 1 - Name of the array to loop through  
text 2 - Trigger to call every time we loop  
text 3 - Trigger to call once we exit the loop  
number 1 (optional) - Delay in ms between loops, default 1000 (1sec) set to -1 for no delay  
number 3 (optional) - Session ID  

### Callback 
text 1 - The decimal from the specified array  
number 2 - Number of times we have looped  (-1 if done)  
number 3 - Session ID  

This is roughly equivalent to the following in (Visual) Basic

```
Dim Array as Float() = GetVNyanArray(Text1)
For Each Value in Array
    CallVNyan(Text2, Value)
Next
CallVNyan(Text3)
```

```_lum_loop_foreacht``` - Loop through a text array  
Works exactly the same as ```_lum_loop_foreach```

## While / DoWhile / Until
```_lum_loop_whileLT``` - While specified decimal is less than specified value (exits once greater than or equal)  
text 1 - Name of the decimal to check  
text 2 - Trigger to call repeatedly while condition is true  
text 3 - Trigger to call once condition is false and loop exited  
number 1 - Target value. Will loop until decimal is no-longer less than this  
number 2 - Divisor. If you need to compare a specific decimal set this. We will divide by it e.g. 1/10 = 0.1  
number 3 - Session ID, will be passed to the triggers we call  

### Callback 
number 2 - Number of times we have looped (-1 if done, -2 if TTL expired)  
number 3 - Session ID  
text 1 - Value of of decimal we're checking

```_lum_loop_whileLE``` - Same but we check less than or equal  
```_lum_loop_whileGT``` - Same but we check if greater than  
```_lum_loop_whileGE``` - Same but we check if greater than or equal  
```_lum_loop_whileEQ``` - Same but we check if the value is exactly equal *This will be rounded to an integer before checking*  
```_lum_loop_whileNE``` - Same but we check if the value is not equal *This will be rounded to an integer before checking*  
```_lum_loop_whileTE``` - Same as EQ but compares text instead of numbers  
```_lum_loop_whileTN``` - Same as NE but compares text instead of numbers

```_lum_loop_dowhileXX``` - where XX can be LE, GT etc. Same as while except guaranteed to run once

```_lum_loop_untilXX``` - Similar to while, except guaranteed to run once, also runs UNTIL specified specified decimal is less than (i.e. untilLE runs if the value is greater than or equal)  

These triggers are roughly equivalent to doing this in (Visual) Basic:
```
# _lum_loop_whileLT
While (GetVNyanParam(text1) < num1)
  CallVNyan(text2)
Wend
CallVNyan(text3)

# _lum_loop_dowhileLT
Do
  CallVNyan(text2)
While (GetVNyanParam(text1) < num1)
CallVNyan(text3)

# _lum_loop_untilLT
Repeat
  CallVNyan(text2)
Until (GetVNyanParam(text1) < num1)
CallVNyan(text3)
```
The name LT, LE, GT, GT, EQ and NE were chosen to match the comparison operators in Powershell

## Delay, SessionID and TTL
While and Until loops have a maximum number of times they can run until they force exit. This can be changed by appending extra parameters  
Call with e.g. ```_lum_loop_whileLE;delay=69;ttl=420;sessionid=8008135```  
Delay - in ms between loops (default 1000ms i.e. 1 second)  
SessionID - Can be specified in the trigger name, (as well as via a node for For loops) and will be output on node3 to allow you to identify a specific loop  
TTL - number of loops before we kill it off anyway (set to -1 to disable (at your own risk) - only applies to while/dowhile/until loops  

## Warnings (please read this)
* Setting the delay to less than two frames (approx 40ms if you are running at a consistent 60fps) can cause unexpected results. VNyan does not always update parameters instantly, so you could e.g. call a trigger an additional time even after setting it to the exit condition. This is less of an issue for For and ForEach loops, provided you only act on the output of the trigger and not on any parameters a delay of 1 should be safe. 
* Overriding or disabling the TTL can cause a loop to run indefinitely. Currently there is no way to stop such a loop (coming in 1.1), so you will need to restart VNyan. You are responsible for ensuring that all loops have a working exit condition.  
* While a For or ForEach loop can be run with a delay of zero. This must only be used for very small loops as they will hang VNyan while they run. If you expect your loop to take more than 10-15ms to process you will cause your model to stutter.  
* To prepare for v1.1. Please do not run multiple loops in parallel set to the same SessionID, and do not rely on loops getting a session ID of zero if one is not specified (see the 1.0 release notes for more info)

## Troubleshooting
Install my VNyan-Debug plugin from https://github.com/LumKitty/VNyan-Debug to see loop status messages in the console
