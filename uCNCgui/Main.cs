namespace uCNCgui;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

public partial class Main : Form
{
	public enum IOPins
	{
		STEP0 = 0,
		STEP1 = 1,
		STEP2 = 2,
		STEP3 = 3,
		STEP4 = 4,
		STEP5 = 5,
		STEP6 = 6,
		STEP7 = 7,
		DIR0 = 8,
		DIR1 = 9,
		DIR2 = 10,
		DIR3 = 11,
		DIR4 = 12,
		DIR5 = 13,
		DIR6 = 14,
		DIR7 = 15,
		STEP0_EN = 16,
		STEP1_EN = 17,
		STEP2_EN = 18,
		STEP3_EN = 19,
		STEP4_EN = 20,
		STEP5_EN = 21,
		STEP6_EN = 22,
		STEP7_EN = 23,
		PWM0 = 24,
		PWM1 = 25,
		PWM2 = 26,
		PWM3 = 27,
		PWM4 = 28,
		PWM5 = 29,
		PWM6 = 30,
		PWM7 = 31,
		PWM8 = 32,
		PWM9 = 33,
		PWM10 = 34,
		PWM11 = 35,
		PWM12 = 36,
		PWM13 = 37,
		PWM14 = 38,
		PWM15 = 39,
		SERVO0 = 40,
		SERVO1 = 41,
		SERVO2 = 42,
		SERVO3 = 43,
		SERVO4 = 44,
		SERVO5 = 45,
		DOUT0 = 46,
		DOUT1 = 47,
		DOUT2 = 48,
		DOUT3 = 49,
		DOUT4 = 50,
		DOUT5 = 51,
		DOUT6 = 52,
		DOUT7 = 53,
		DOUT8 = 54,
		DOUT9 = 55,
		DOUT10 = 56,
		DOUT11 = 57,
		DOUT12 = 58,
		DOUT13 = 59,
		DOUT14 = 60,
		DOUT15 = 61,
		DOUT16 = 62,
		DOUT17 = 63,
		DOUT18 = 64,
		DOUT19 = 65,
		DOUT20 = 66,
		DOUT21 = 67,
		DOUT22 = 68,
		DOUT23 = 69,
		DOUT24 = 70,
		DOUT25 = 71,
		DOUT26 = 72,
		DOUT27 = 73,
		DOUT28 = 74,
		DOUT29 = 75,
		DOUT30 = 76,
		DOUT31 = 77,
		LIMIT_X = 100,
		LIMIT_Y = 101,
		LIMIT_Z = 102,
		LIMIT_X2 = 103,
		LIMIT_Y2 = 104,
		LIMIT_Z2 = 105,
		LIMIT_A = 106,
		LIMIT_B = 107,
		LIMIT_C = 108,
		PROBE = 109,
		ESTOP = 110,
		SAFETY_DOOR = 111,
		HOLD = 112,
		CYCLE_RESUME = 113,
		ANALOG0 = 114,
		ANALOG1 = 115,
		ANALOG2 = 116,
		ANALOG3 = 117,
		ANALOG4 = 118,
		ANALOG5 = 119,
		ANALOG6 = 120,
		ANALOG7 = 121,
		ANALOG8 = 122,
		ANALOG9 = 123,
		ANALOG10 = 124,
		ANALOG11 = 125,
		ANALOG12 = 126,
		ANALOG13 = 127,
		ANALOG14 = 128,
		ANALOG15 = 129,
		DIN0 = 130,
		DIN1 = 131,
		DIN2 = 132,
		DIN3 = 133,
		DIN4 = 134,
		DIN5 = 135,
		DIN6 = 136,
		DIN7 = 137,
		DIN8 = 138,
		DIN9 = 139,
		DIN10 = 140,
		DIN11 = 141,
		DIN12 = 142,
		DIN13 = 143,
		DIN14 = 144,
		DIN15 = 145,
		DIN16 = 146,
		DIN17 = 147,
		DIN18 = 148,
		DIN19 = 149,
		DIN20 = 150,
		DIN21 = 151,
		DIN22 = 152,
		DIN23 = 153,
		DIN24 = 154,
		DIN25 = 155,
		DIN26 = 156,
		DIN27 = 157,
		DIN28 = 158,
		DIN29 = 159,
		DIN30 = 160,
		DIN31 = 161,
		TX = 200,
		RX = 201,
		USB_DM = 202,
		USB_DP = 203,
		SPI_CLK = 204,
		SPI_SDI = 205,
		SPI_SDO = 206
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct VirtualMap_T
	{
		public System.UInt32 SpecialOutputs;
		public System.UInt32 Outputs;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public System.Byte[] PWM;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public System.Byte[] Servos;
		public System.UInt32 SpecialInputs;
		public System.UInt32 Inputs;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public System.Byte[] Analogs;
	}

	public Main()
	{
		InitializeComponent();
	}

	delegate void OutputCheckBoxCallback(string id, bool value);

	void OutputCheckBox(string id, bool value)
	{
		Control[] c = this.Controls.Find(id, true);
		if (c.Length != 0)
		{
			CheckBox chk = (CheckBox)c[0];
			if (chk.InvokeRequired)
			{
				OutputCheckBoxCallback d = new OutputCheckBoxCallback(OutputCheckBox);
				this.Invoke(d, new object[] { id, value });
			}
			else
			{
				chk.Checked = value;
			}

		}
	}

	bool InputCheckBox(string id)
	{
		Control[] c = this.Controls.Find(id, true);
		if (c.Length != 0)
		{
			CheckBox chk = (CheckBox)c[0];
			return chk.Checked;
		}

		return false;
	}

	delegate void OutputProgressBarCallback(string id, byte value);

	void OutputProgressBar(string id, byte value)
	{
		Control[] c = this.Controls.Find(id, true);
		if (c.Length != 0)
		{
			ProgressBar pgr = (ProgressBar)c[0];
			if (pgr.InvokeRequired)
			{
				OutputProgressBarCallback d = new OutputProgressBarCallback(OutputProgressBar);
				this.Invoke(d, new object[] { id, value });
			}
			else
			{
				pgr.Value = (int)value;
			}

		}
	}

	delegate void StatusInfoTextCallback(ToolStripStatusLabel c, string value);
	void StatusInfoText(ToolStripStatusLabel c, string value)
	{
		//if (c.InvokeRequired)
		//{
		//    StatusInfoTextCallback d = new StatusInfoTextCallback(StatusInfoText);
		//    this.Invoke(d, new object[] { c, value });
		//}
		//else
		//{
		c.Text = value;
		//}
	}

	private long _lastTimeStamp = 0;
	private bool _quit = false;
	private void pipeClient_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
	{
		while (!e.Cancel && !_quit)
		{
			StatusInfoText(connectionStatus, "Disconnected");
			StatusInfoText(connectionFreq, "Update frequency: N/A");

			var pipeClient =
					new NamedPipeClientStream(".", "ucncio",
						PipeDirection.InOut, PipeOptions.None,
						TokenImpersonationLevel.None);

			StatusInfoText(connectionStatus, "Waiting for client");

			try
			{
				pipeClient.Connect(1000);

				StatusInfoText(connectionStatus, "Connected");

				Stream pipeStream = pipeClient;

				// Validate the server's signature string.
				while (!e.Cancel && !_quit)
				{
					_lastTimeStamp = DateTime.Now.Ticks;
					int len;
					len = 56;
					byte[] buffer = new byte[len];
					pipeStream.Read(buffer, 0, len);
					GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
					VirtualMap_T pins;
					try
					{
						pins = (VirtualMap_T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(VirtualMap_T));
					}
					finally
					{
						handle.Free();
					}

					//update special ouputs
					for (int i = (int)IOPins.STEP0; i <= (int)IOPins.STEP7_EN; i++)
					{
						IOPins io = (IOPins)i;
						int offset = i - (int)IOPins.STEP0;
						OutputCheckBox(io.ToString(), ((pins.SpecialOutputs & (1U << i)) != 0));
					}

					//update ouputs
					for (int i = (int)IOPins.DOUT0; i <= (int)IOPins.DOUT31; i++)
					{
						IOPins io = (IOPins)i;
						int offset = i - (int)IOPins.DOUT0;
						OutputCheckBox(io.ToString(), ((pins.Outputs & (1U << offset)) != 0));
					}

					//update PWM
					for (int i = 0; i < 16; i++)
					{
						byte value = pins.PWM[i];
						IOPins io = (IOPins)(i + (int)IOPins.PWM0);
						OutputProgressBar(io.ToString(), value);
					}

					//update PWM
					for (int i = 0; i < 6; i++)
					{
						byte value = pins.Servos[i];
						IOPins io = (IOPins)(i + (int)IOPins.SERVO0);
						OutputProgressBar(io.ToString(), value);
					}

					//read inputs
					for (int i = (int)IOPins.LIMIT_X; i <= (int)IOPins.CYCLE_RESUME; i++)
					{
						IOPins io = (IOPins)i;
						int offset = i - (int)IOPins.LIMIT_X;
						if (InputCheckBox(io.ToString()))
						{
							pins.SpecialInputs |= (1U << offset);
						}
						else
						{
							pins.SpecialInputs &= ~(1U << offset);
						}
					}

					for (int i = (int)IOPins.DIN0; i <= (int)IOPins.DIN31; i++)
					{
						IOPins io = (IOPins)i;
						int offset = i - (int)IOPins.DIN0;
						if (InputCheckBox(io.ToString()))
						{
							pins.Inputs |= (1U << offset);
						}
						else
						{
							pins.Inputs &= ~(1U << offset);
						}
					}

					//read analogs
					for (int i = (int)IOPins.ANALOG0; i <= (int)IOPins.ANALOG15; i++)
					{
						IOPins io = (IOPins)i;
						Control[] c = this.Controls.Find(io.ToString(), true);
						if (c.Length != 0)
						{
							NumericUpDown num = (NumericUpDown)c[0];
							byte value = (byte)num.Value;
							pins.Analogs[i - (int)IOPins.ANALOG0] = value;
						}
					}

					handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
					try
					{
						Marshal.StructureToPtr(pins, handle.AddrOfPinnedObject(), true);
						pipeStream.Write(buffer, 0, 56);
						pipeStream.Flush();
					}
					catch
					{
						break;
					}
					finally
					{
						handle.Free();

					}

					long current = DateTime.Now.Ticks - _lastTimeStamp;

					int freq = (int)Math.Round(1000.0 / TimeSpan.FromTicks(current).TotalMilliseconds);
					StatusInfoText(connectionFreq, "Update frequency: " + freq + "Hz");
				}

				pipeStream.Close();
				pipeClient.Close();
			}
			catch
			{

			}
		}
	}

	private void Main_Load(object sender, EventArgs e)
	{
		for (int i = 0; i < this.Controls.Count; i++)
		{
			if (this.Controls[i] is GroupBox)
			{
				for (int j = 0; j < this.Controls[i].Controls.Count; j++)
				{
					if (this.Controls[i].Controls[j] is CheckBox)
					{
						this.Controls[i].Controls[j].Text = this.Controls[i].Controls[j].Name;
					}
				}
			}
		}
		pipeClient.RunWorkerAsync();
	}

	private void Main_FormClosing(object sender, FormClosingEventArgs e)
	{
		pipeClient.CancelAsync();
		_quit = true;
		while (pipeClient.CancellationPending)
		{
			Application.DoEvents();
		}
	}
}
