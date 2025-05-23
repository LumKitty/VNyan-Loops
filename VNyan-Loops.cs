﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VNyan_Loops
{
    public class VNyan_Loops : MonoBehaviour, VNyanInterface.ITriggerHandler
    {
        const string Version = "1.1-RC2";
        Dictionary<int, LoopStatus> ActiveLoops = new Dictionary<int, LoopStatus>();
        
        enum LoopStatus
        {
            //Created,
            Running,
            CancelRequested,
            Finished
        }

        enum Operation
        {
            Unknown,
            LT,
            LE,
            GT,
            GE,
            EQ,
            NE,
            TE,
            TN
        }
        
        int Round(float Value)
        {
            return (int)Math.Floor(Value);
        }
        int Round(decimal Value)
        {
            return (int)Math.Floor(Value);
        }
        int GetUniqueSessionID(int SessionID)
        {
            if (SessionID == 0) {
                bool done = false;
                SessionID = 10000;
                do
                {
                    SessionID++;
                    if (ActiveLoops.ContainsKey(SessionID))
                    {
                        if (ActiveLoops[SessionID] == LoopStatus.Finished)
                        {
                            ActiveLoops[SessionID] = LoopStatus.Running;
                            done = true;
                        }
                    }
                    else
                    {
                        ActiveLoops.Add(SessionID, LoopStatus.Running);
                        done = true;
                    }
                } while (!done);
            }
            else
            {
                if (ActiveLoops.ContainsKey(SessionID))
                {
                    if (ActiveLoops[SessionID] == LoopStatus.Finished)
                    {
                        ActiveLoops[SessionID] = LoopStatus.Running;
                    }
                    else
                    {
                        KillLoop(SessionID, true);
                    }
                }
                else
                {
                    ActiveLoops.Add(SessionID, LoopStatus.Running);
                }
            }
            
            return SessionID;
        }

        /*void CheckLoopStartup(int SessionID)
        {
            Log("Checking session log for " + SessionID);
            if (ActiveLoops[SessionID] != LoopStatus.Created)
            {
                KillLoop(SessionID, true);
            }
            ActiveLoops[SessionID] = LoopStatus.Running;
        }*/

        void CallVNyan(string TriggerName, int int1, int int2, int int3, string Text1, string Text2, string Text3)
        {
            if (TriggerName.Length > 0)
            {
                VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger(TriggerName, int1, int2, int3, Text1, Text2, Text3);
            }
            else
            {
                VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger("_lum_miu_error", 0, 0, 0, "Invalid trigger name", "", "");
            }
        }
        float GetVNyanDecimal(string DecimalName)
        {
            return VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterFloat(DecimalName);
        }
        string GetVNyanText(string TextParamName)
        {
            return VNyanInterface.VNyanInterface.VNyanParameter.getVNyanParameterString(TextParamName);
        }
        void Log(string Message)
        {
            CallVNyan("_lum_dbg_raw", 0, 0, 0, Message, "", "");
        }
        void ErrorHandler(Exception e)
        {
            CallVNyan("_lum_dbg_err", 0, 0, 0, "VNyan-Loops", e.ToString(), "");
        }
        public void Awake()
        {
            try
            {
                VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }
        bool CompareDecimals(float lhs, float rhs, Operation oper)
        {
            Log("LOOP: Comparing " + lhs.ToString() + " "+oper.ToString()+" " + rhs.ToString());
            switch (oper)
            {
                case Operation.LT: return (lhs < rhs);
                case Operation.LE: return (lhs <= rhs);
                case Operation.GT: return (lhs > rhs);
                case Operation.GE: return (lhs >= rhs);
                case Operation.EQ: return ((int)Math.Round(lhs, 0) == (int)Math.Round(rhs, 0));
                case Operation.NE: return ((int)Math.Round(lhs, 0) != (int)Math.Round(rhs, 0));
                case Operation.Unknown: Log("Unknown operation type passed"); return true;
            }
            Log("No operation passed. If you see this something bad happened!");
            return true;
        }
        bool CompareText(string lhs, string rhs, Operation oper)
        {
            Log("LOOP: Comparing " + lhs.ToString() + " " + oper.ToString() + " " + rhs.ToString());
            switch (oper)
            {
                case Operation.TE: return (lhs == rhs);
                case Operation.TN: return (lhs != rhs);
                case Operation.Unknown: Log("Unknown operation type passed"); return true;
            }
            Log("No operation passed. If you see this something bad happened!");
            return true;
        }
        async Task WhileLoop(string LoopTrigger, string ExitTrigger, string DecimalName, float TargetValue, int Delay, int TTL, Operation Operation, int SessionID, bool Until)
        { // Number version
            try
            {
                const string LoopType = "WhileNumber";
                SessionID = GetUniqueSessionID(SessionID);
                int Runs = 0;
                if (Until) { Log("Until loop started"); } else { Log("Until loop started"); }
                Log("LoopTrigger=" + LoopTrigger + "; ExitTrigger=" + ExitTrigger + "Param=" + DecimalName + "; TargetValue=" + TargetValue.ToString() + "; Operation=" + Operation.ToString() + "; SessionID=" + SessionID.ToString());
                DateTime WaitTime = DateTime.Now;
                if (Until)
                {
                    CallVNyan(LoopTrigger, Round(GetVNyanDecimal(DecimalName)), Runs, SessionID, GetVNyanDecimal(DecimalName).ToString(), "", LoopType);
                    Runs++;
                    WaitTime = WaitTime.AddMilliseconds(Delay); Thread.Sleep(WaitTime - DateTime.Now);
                }
                if (ActiveLoops[SessionID] != LoopStatus.CancelRequested)
                {
                    if (TTL > 0)
                    {
                        while (Runs < TTL && !CompareDecimals(GetVNyanDecimal(DecimalName), TargetValue, Operation))
                        {
                            CallVNyan(LoopTrigger, Round(GetVNyanDecimal(DecimalName)), Runs, SessionID, GetVNyanDecimal(DecimalName).ToString(), DecimalName, LoopType);
                            Runs++;
                            WaitTime = WaitTime.AddMilliseconds(Delay); Thread.Sleep(WaitTime - DateTime.Now);
                            if (ActiveLoops[SessionID] == LoopStatus.CancelRequested)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        while (!CompareDecimals(GetVNyanDecimal(DecimalName), TargetValue, Operation))
                        {
                            CallVNyan(LoopTrigger, Round(GetVNyanDecimal(DecimalName)), Runs, SessionID, GetVNyanDecimal(DecimalName).ToString(), DecimalName, LoopType);
                            Runs++;
                            WaitTime = WaitTime.AddMilliseconds(Delay); Thread.Sleep(WaitTime - DateTime.Now);
                            if (ActiveLoops[SessionID] == LoopStatus.CancelRequested)
                            {
                                break;
                            }
                        }
                        Runs = TTL - 1; // To ensure the "ended normally" condition is met when running without TTL
                    }
                }
                if (ActiveLoops[SessionID] == LoopStatus.CancelRequested)
                {
                    Log("Loop " + SessionID.ToString() + " cancelled.");
                }
                else
                {
                    if (Runs < TTL)
                    {
                        Log("Loop " + SessionID.ToString() + " ended normally.");
                        CallVNyan(ExitTrigger, 0, -1, SessionID, GetVNyanDecimal(DecimalName).ToString(), DecimalName, LoopType);
                    }
                    else
                    {
                        Log("Loop " + SessionID.ToString() + " ended (TTL expired)");
                        CallVNyan(ExitTrigger, 0, -2, SessionID, GetVNyanDecimal(DecimalName).ToString(), DecimalName, LoopType);
                    }
                }
                ActiveLoops[SessionID] = LoopStatus.Finished;
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }
        async Task WhileLoop(string LoopTrigger, string ExitTrigger, string TextParamName, string TargetValue, int Delay, int TTL, Operation Operation, int SessionID, bool Until)
        { // Text version
            try
            {
                const string LoopType = "WhileText";
                SessionID = GetUniqueSessionID(SessionID);
                int Runs = 0;
                if (Until) { Log("Until loop started"); } else { Log("Until loop started"); }
                Log("LoopTrigger=" + LoopTrigger + "; ExitTrigger=" + ExitTrigger + "Param=" + TextParamName + "; TargetValue=" + TargetValue.ToString() + "; Operation=" + Operation.ToString() + "; SessionID=" + SessionID.ToString());
                DateTime WaitTime = DateTime.Now;

                if (Until)
                {
                    CallVNyan(LoopTrigger, 0, Runs, SessionID, GetVNyanText(TextParamName).ToString(), TextParamName, LoopType);
                    Runs++;
                    WaitTime = WaitTime.AddMilliseconds(Delay); Thread.Sleep(WaitTime - DateTime.Now);
                }
                if (ActiveLoops[SessionID] != LoopStatus.CancelRequested)
                {
                    if (TTL > 0)
                    {
                        while (Runs < TTL && !CompareText(GetVNyanText(TextParamName), TargetValue, Operation))
                        {
                            CallVNyan(LoopTrigger, 0, Runs, SessionID, GetVNyanText(TextParamName), TextParamName, LoopType);
                            Runs++;
                            WaitTime = WaitTime.AddMilliseconds(Delay); Thread.Sleep(WaitTime - DateTime.Now);
                        }
                    }
                    else
                    {
                        while (!CompareText(GetVNyanText(TextParamName), TargetValue, Operation))
                        {
                            CallVNyan(LoopTrigger, 0, Runs, SessionID, GetVNyanText(TextParamName), TextParamName, LoopType);
                            Runs++;
                            WaitTime = WaitTime.AddMilliseconds(Delay); Thread.Sleep(WaitTime - DateTime.Now);
                        }
                        Runs = TTL - 1; // To ensure the "ended normally" condition is met when running without TTL
                    }
                }
                if (ActiveLoops[SessionID] == LoopStatus.CancelRequested)
                {
                    Log("Loop " + SessionID.ToString() + " cancelled.");
                }
                else
                {
                    if (Runs < TTL)
                    {
                        Log("Loop " + SessionID.ToString() + " ended normally.");
                        CallVNyan(ExitTrigger, Runs, -1, SessionID, GetVNyanText(TextParamName), "", LoopType);
                    }
                    else
                    {
                        Log("Loop " + SessionID.ToString() + " ended (TTL expired)");
                        CallVNyan(ExitTrigger, Runs, -2, SessionID, GetVNyanText(TextParamName), "", LoopType);
                    }
                }
                ActiveLoops[SessionID] = LoopStatus.Finished;
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }
        async Task ForEachLoop(string LoopTrigger, string ExitTrigger, string[] TextValues, int Delay, int SessionID)
        {
            try
            {
                const string LoopType = "ForEach";
                SessionID = GetUniqueSessionID(SessionID);
                int Runs = 0;
                int ConvertedValue;
                Log("LoopTrigger=" + LoopTrigger + "; ExitTrigger=" + ExitTrigger + "; ArrayEntries=" + TextValues.Count() + ";Delay=" + Delay + "; SessionID=" + SessionID.ToString());
                DateTime WaitTime = DateTime.Now;

                foreach (string TextValue in TextValues)
                {
                    try
                    {
                        ConvertedValue = Round(Decimal.Parse(TextValue));
                    }
                    catch
                    {
                        ConvertedValue = 0;
                    }
                    CallVNyan(LoopTrigger, ConvertedValue, Runs, SessionID, TextValue, "", LoopType);
                    Runs++;
                    WaitTime = WaitTime.AddMilliseconds(Delay); Thread.Sleep(WaitTime - DateTime.Now);
                    if (ActiveLoops[SessionID] == LoopStatus.CancelRequested)
                    {
                        break;
                    }
                }
                if (ActiveLoops[SessionID] == LoopStatus.CancelRequested)
                {
                    Log("Loop " + SessionID.ToString() + " cancelled.");
                }
                else
                {
                    Log("Loop " + SessionID.ToString() + " ended normally.");
                    CallVNyan(ExitTrigger, 0, -1, SessionID, "", "", LoopType);
                }
                ActiveLoops[SessionID] = LoopStatus.Finished;
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }
        async Task ForLoop(string LoopTrigger, string ExitTrigger, string DecimalName, int StartValue, int TargetValue, int Step, int Delay, Operation Operation, int SessionID)
        {
            const string LoopType = "For";
            SessionID = GetUniqueSessionID(SessionID);
            Log("For loop "+SessionID.ToString()+" from " + StartValue.ToString() + " to " + TargetValue.ToString() + "; Step: " + Step.ToString() + "; Comparison: " + Operation.ToString());
            int Runs = 0;
            DateTime WaitTime = DateTime.Now;
            for (int n = StartValue; !CompareDecimals(n, TargetValue, Operation); n = n + Step)
            {
                if (DecimalName.Length > 0) { VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterFloat(DecimalName, (float)n); }
                CallVNyan(LoopTrigger, n, Runs, SessionID, GetVNyanDecimal(DecimalName).ToString(), "", LoopType);
                Runs++;
                WaitTime = WaitTime.AddMilliseconds(Delay); Thread.Sleep(WaitTime - DateTime.Now);
                if (ActiveLoops[SessionID] == LoopStatus.CancelRequested)
                {
                    break;
                }
            }
            if (ActiveLoops[SessionID] == LoopStatus.CancelRequested)
            {
                Log("Loop "+SessionID.ToString()+" cancelled.");
            }
            else
            {
                Log("Loop "+SessionID.ToString()+" ended normally.");
                CallVNyan(ExitTrigger, TargetValue, -1, SessionID, GetVNyanDecimal(DecimalName).ToString(), "", LoopType);
            }
            ActiveLoops[SessionID] = LoopStatus.Finished;
        }

        void KillLoop(int SessionID, bool WaitForTermination)
        {
            Log("KillLoop called. SessionID: " + SessionID.ToString());
            if (ActiveLoops.ContainsKey(SessionID))
            {
                if (ActiveLoops[SessionID] != LoopStatus.Finished) {
                    Log("Active loop found for sessionID: " + SessionID.ToString());
                    ActiveLoops[SessionID] = LoopStatus.CancelRequested;
                    if (WaitForTermination)
                    {
                        Log("Killing loop " + SessionID + "and waiting for termination");
                        while (ActiveLoops[SessionID] != LoopStatus.Finished)
                        {
                            Thread.Sleep(50);
                        }
                    }
                    else
                    {
                        Log("Killing loop " + SessionID);
                    }
                } 
            } 
            else
            {
                Log("Couldn't kill loop " + SessionID + " as it does not exist");
            }
        }

        public void triggerCalled(string name, int int1, int int2, int int3, string text1, string text2, string text3)
        {
            try
            {
                if (name.Length > 11 && name.Substring(0, 10) == "_lum_loop_")
                {
                    Log("Loops detected");
                    string[] Parameters = name.Split(';');  // _lum_loop_whileLT;delay=100;ttl=50
                    float TargetValue;
                    string TextTargetValue = null;
                    string[] ParamTemp;
                    string CompType;
                    string Delim = ",";
                    Operation Oper = Operation.Unknown;

                    int Delay = 1000;
                    int TTL = 1000;
                    int SessionID = 0;

                    foreach (string Parameter in Parameters.Skip(1))
                    {
                        if (Parameter.Contains('='))
                        {
                            Log("Parsing: " + Parameter);
                            ParamTemp = Parameter.Split('=');   //ttl=100
                            switch (ParamTemp[0].ToLower())
                            {
                                case "ttl":
                                    TTL = int.Parse(ParamTemp[1]);
                                    Log("TTL set to: " + TTL.ToString());
                                break;
                                case "delay":
                                    Delay = int.Parse(ParamTemp[1]);
                                    Log("Delay set to: " + Delay.ToString());
                                break;
                                case "sessionid":
                                    SessionID = int.Parse(ParamTemp[1]);
                                    Log("Session ID set to: " + SessionID.ToString());
                                break;
                                case "val":
                                    TextTargetValue = ParamTemp[1];
                                    Log("Target value set to: " + TextTargetValue);
                                break;
                                case "delim":
                                    Delim = ParamTemp[1];
                                    Log("CSV delimeter set to: " + Delim);
                                break;
                            }
                        }
                    }

                    string LoopType = Parameters[0].Substring(9).ToLower();
                    Log("Checking: " + LoopType);
                    switch (LoopType)
                    {
                        case "_kill":
                            if (SessionID == 0)
                            {
                                SessionID = int3;
                            }
                            KillLoop(SessionID, false);
                        break;
                        case "_for":

                            Log("For loop detected");
                            if (int2 < int1)
                            {
                                if (int3 >= 0) { int3 = -1; }
                                
                                Task.Run(() => ForLoop(text2, text3, text1, int1, int2, int3, Delay, Operation.LT, SessionID));
                            }
                            else if (int2 > int1)
                            {
                                if (int3 <= 0) { int3 = 1; }
                                Task.Run(() => ForLoop(text2, text3, text1, int1, int2, int3, Delay, Operation.GT, SessionID));
                            }
                            else
                            {
                                Log("Loop will never complete");
                            }
                        break;
                        case "_foreachcsv":
                            if (SessionID == 0) { SessionID = int3; }
                            if (int1 != 0)
                            {
                                Delay = int1;
                            }
                            else if (int1 < 0)
                            {
                                Delay = 0;
                            }
                            string[] TextValues = text1.Split(new string[] { Delim }, StringSplitOptions.None);
                            Task.Run(() => ForEachLoop(text2, text3, TextValues, Delay, SessionID));
                        break;

                        default:
                            if (SessionID == 0) { SessionID = int3; }
                            CompType = Parameters[0].Substring(Parameters[0].Length - 2).ToUpper();
                            LoopType = Parameters[0].Substring(9, Parameters[0].Length - 11).ToLower();
                            Log("Processing loop type: " + LoopType);
                            if (TextTargetValue is null)
                            {
                                if (int2 != 0)
                                {
                                    TargetValue = (float)int1 / (float)int2;
                                }
                                else
                                {
                                    TargetValue = (float)int1;
                                }
                            }
                            else
                            {
                                try
                                {
                                    TargetValue = float.Parse(TextTargetValue);
                                }
                                catch
                                {
                                    Log("Could not convert " + TextTargetValue.ToString() + " to a decimal. Hopefully you're doing a text comparison!");
                                    TargetValue = 0;
                                }
                            }
                            Log("Processing comparison type: " + CompType);
                            switch (Parameters[0].Substring(Parameters[0].Length - 2).ToUpper())
                            {
                                case "LT": Oper = Operation.GE; break;
                                case "LE": Oper = Operation.GT; break;
                                case "GT": Oper = Operation.LE; break;
                                case "GE": Oper = Operation.LT; break;
                                case "EQ": Oper = Operation.NE; break;
                                case "NE": Oper = Operation.EQ; break;
                                case "TE": Oper = Operation.TN; break;
                                case "TN": Oper = Operation.TE; break;
                            }
                            switch (LoopType)
                            {
                                case "_while":

                                    if (Oper != Operation.Unknown)
                                    {
                                        if (Oper != Operation.TE && Oper != Operation.TN)
                                        {
                                            Task.Run(() => WhileLoop(text2, text3, text1, TargetValue, Delay, TTL, Oper, SessionID, false));
                                        }
                                        else
                                        {
                                            Task.Run(() => WhileLoop(text2, text3, text1, TextTargetValue, Delay, TTL, Oper, SessionID, false));
                                        }
                                    }
                                    else
                                    {
                                        CallVNyan("_lum_miu_error", int1, int2, int3, "Unknown while operation requested" + name + ", " + text1, text2, text3);
                                    }
                                    break;
                                case "_dowhile":
                                    if (Oper != Operation.Unknown)
                                    {
                                        if (Oper != Operation.TE && Oper != Operation.TN)
                                        {
                                            Task.Run(() => WhileLoop(text2, text3, text1, TargetValue, Delay, TTL, Oper, SessionID, true));
                                        }
                                        else
                                        {
                                            Task.Run(() => WhileLoop(text2, text3, text1, TextTargetValue, Delay, TTL, Oper, SessionID, true));
                                        }
                                    }
                                    else
                                    {
                                        CallVNyan("_lum_miu_error", int1, int2, int3, "Unknown dowhile operation requested" + name + ", " + text1, text2, text3);
                                    }
                                    break;
                                case "_until":
                                    Log("Processing comparison type: " + CompType);
                                    switch (Oper)
                                    {
                                        case Operation.GE: Oper = Operation.LT; break;
                                        case Operation.GT: Oper = Operation.LE; break;
                                        case Operation.LE: Oper = Operation.GT; break;
                                        case Operation.LT: Oper = Operation.GE; break;
                                        case Operation.NE: Oper = Operation.EQ; break;
                                        case Operation.EQ: Oper = Operation.NE; break;
                                        case Operation.TE: Oper = Operation.TN; break;
                                        case Operation.TN: Oper = Operation.TE; break;
                                    }
                                    if (Oper != Operation.Unknown)
                                    {
                                        if (Oper != Operation.TE && Oper != Operation.TN)
                                        {
                                            Task.Run(() => WhileLoop(text2, text3, text1, TargetValue, Delay, TTL, Oper, int3, true));
                                        }
                                        else
                                        {
                                            Task.Run(() => WhileLoop(text2, text3, text1, TextTargetValue, Delay, TTL, Oper, int3, true));
                                        }
                                    }
                                    else
                                    {
                                        CallVNyan("_lum_miu_error", int1, int2, int3, "Unknown until operation requested" + name + ", " + text1, text2, text3);
                                    }
                                break;
                            }
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }
    }
}
