﻿/* VMukti 2.0 -- An Open Source Unified Communications Engine
*
* Copyright (C) 2008 - 2009, VMukti Solutions Pvt. Ltd.
*
* Hardik Sanghvi <hardik@vmukti.com>
*
* See http://www.vmukti.com for more information about
* the VMukti project. Please do not directly contact
* any of the maintainers of this project for assistance;

*/
using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using ToneDetect.Business.Fourier;

namespace ToneDetect.Business.Detect
{
	internal class ToneDetect
	{
		private int counter;

		protected string streamName;

		internal ToneDetect()
		{
			counter = 0;
		}

		/// <summary>
		/// Determine the maximum frequency given the set of frequency bins
		/// </summary>
		/// <param name="frequencyBins">set of bins to find max freq in</param>
		/// <returns>the frequency of the bin with the highest energy</returns>
		static internal double FindMaxFrequency( FrequencyBin[] frequencyBins )
		{
			double retval;
			double max;

			retval = 0.0;
			max = 0.0;
			for( int i = 0; i < frequencyBins.Length; ++i )
			{
				if( frequencyBins[i].Energy > max )
				{
					max = frequencyBins[i].Energy;
					retval = frequencyBins[i].Frequency;
				}
			}

			return retval;
		}
	}
}
