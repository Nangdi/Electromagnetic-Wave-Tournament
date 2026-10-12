using System;

// 선택 스위치 컨트롤러의 RS232 수신 처리.
// 수신 문자열을 Setting.json 의 leftSwitchCommand / rightSwitchCommand 와 비교해 SwitchInput으로 넘긴다.
public class TournamentSerialManager : SerialPortManager
{
    protected override void ReceivedData(int controllerId, string data)
    {
        if (string.IsNullOrEmpty(data) || SwitchInput.Instance == null) return;

        string message = data.Trim();
        var config = TournamentConfig.Current;

        if (string.Equals(message, config.leftSwitchCommand?.Trim(), StringComparison.OrdinalIgnoreCase))
            SwitchInput.Instance.Press(SwitchSide.Left);
        else if (string.Equals(message, config.rightSwitchCommand?.Trim(), StringComparison.OrdinalIgnoreCase))
            SwitchInput.Instance.Press(SwitchSide.Right);
    }
}
