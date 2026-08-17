using System;

namespace CyberHeistButuan.Engine
{
    public enum DetectionState
    {
        Undetected,
        Suspicious,
        Detected,
        In_Encounter
    }

    public class Detection_System
    {
        public DetectionState CurrentState { get; private set; } = DetectionState.Undetected;

        private int _suspiciousTurnsRemaining = 0;
        private int _consecutiveFailedChecksInSuspicious = 0;
        private int _detectedTurnsRemaining = 0;

        public void SetState(DetectionState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;

            if (newState == DetectionState.Suspicious)
            {
                _suspiciousTurnsRemaining = 3; // Standard 3 turns to cool down
                _consecutiveFailedChecksInSuspicious = 0;
            }
            else if (newState == DetectionState.Detected)
            {
                _detectedTurnsRemaining = 2; // 2 turn window before combat begins
            }
        }

        public void RecordCheckResult(bool isSuccess)
        {
            if (CurrentState == DetectionState.Undetected && !isSuccess)
            {
                SetState(DetectionState.Suspicious);
            }
            else if (CurrentState == DetectionState.Suspicious)
            {
                if (!isSuccess)
                {
                    _consecutiveFailedChecksInSuspicious++;
                    if (_consecutiveFailedChecksInSuspicious >= 2)
                    {
                        SetState(DetectionState.Detected);
                    }
                }
                else
                {
                    _consecutiveFailedChecksInSuspicious = 0;
                }
            }
        }

        public void ProcessTurn()
        {
            if (CurrentState == DetectionState.Suspicious)
            {
                _suspiciousTurnsRemaining--;
                if (_suspiciousTurnsRemaining <= 0)
                {
                    SetState(DetectionState.Undetected);
                }
            }
            else if (CurrentState == DetectionState.Detected)
            {
                _detectedTurnsRemaining--;
                if (_detectedTurnsRemaining <= 0)
                {
                    SetState(DetectionState.In_Encounter);
                }
            }
        }

        public void ModifySuspiciousTimer(int delta)
        {
            _suspiciousTurnsRemaining = Math.Max(0, _suspiciousTurnsRemaining + delta);
        }

        public void EscapedToSuspicious()
        {
            SetState(DetectionState.Suspicious);
        }

        public int GetSuspiciousTurnsRemaining() => _suspiciousTurnsRemaining;
        public int GetDetectedTurnsRemaining() => _detectedTurnsRemaining;
    }
}