# For, While and Until loops for VNyan

```_lum_loop_whileLT``` - While specified decimal is less than specified value (exits once greater than or equal)  
text 1 - Name of the decimal to check  
text 2 - Trigger to call while condition is true  
text 3 - Trigger to call once condition is false and loop exited  
number 1 - Target value. Will loop until decimal is no-longer less than this  
number 2 - Divisor. If you need to compare a specific decimal set this. We will divide by it e.g. 1/10 = 0.1  
number 3 - Session ID, will be passed to the triggers we call  

Callback:  
number 1 - Number of times we have looped  
number 2 -
  0 - We are looping
  1 - This is a call to the exit trigger
  2 - This is a call to the exit trigger because TTL expired
number 3 - Session ID
text 1 - Value of of decimal we're checking

```_lum_loop_whileLE``` - Same but we check less than or equal  
```_lum_loop_whileGT``` - Same but we check if greater than  
```_lum_loop_whileGE``` - Same but we check if greater than or equal  
```_lum_loop_whileEQ``` - Same but we check if the value is exactly equal *This will be rounded to an integer before checking*  
```_lum_loop_whileNE``` - Same but we check if the value is not equal *This will be rounded to an integer before checking*  

```_lum_loop_untilLT``` - Similar to while, except guaranteed to run once, also runs UNTIL specified specified decimal is less than (runs if greater than or equal)  
```_lum_loop_untilLE```  
```_lum_loop_untilGT```  
```_lum_loop_untilGE```  
```_lum_loop_untilNE```  

## Delay and TTL
Call with e.g. ```_lum_loop_whileLE;delay=69;ttl=420```
Delay - in ms between loops
TTL - number of loops before we kill it off anyway
