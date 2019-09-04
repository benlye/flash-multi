# Serial Monitor
The Serial Monitor can be used to view debug information emitted by a Multimodule which has been flashed with firmware with `DEBUG_SERIAL` enabled.

This can be useful when debugging complex firmware issues, or when capturing data using the Multimodule's [XN297L dump](https://github.com/pascallanger/DIY-Multiprotocol-TX-Module/blob/master/docs/Advanced_XN297Ldump.md) feature to capture data from new transmitters so that new protocols can be added.

<p align="center">
  <img src="/img/serial-monitor.jpg">
</p>

## Use
To use the Serial Monitor simply select the serial (COM) port that the Multimodule is connected to and click the **Serial Monitor** button.

When the Serial Monitor window opens it will open the serial port connection automatically.  Once the window is open the serial port connection can be closed and re-opened as needed.

If a firmware upload is started while the Serial Monitor window is open the connection will be automatically closed to allow the upload to occur, then be re-opened when the upload is complete.

Content of the Serial Monitor window can be saved to a log file by clicking the **Save** button
