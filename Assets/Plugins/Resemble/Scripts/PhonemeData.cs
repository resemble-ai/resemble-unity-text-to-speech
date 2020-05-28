using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    [System.Serializable]
    public class PhonemeData
    {

        public PhonemesTimeStamps raw;
        public Phonemes refined;

        public PhonemeData(PhonemesTimeStamps raw, PhonemeTable table)
        {
            this.raw = raw;
            refined = table == null || raw.end_times == null ? null : table.RefineData(raw);
        }

        public void SetData(PhonemesTimeStamps raw, PhonemeTable table)
        {
            this.raw = raw;
            refined = table == null || raw.end_times == null ? null : table.RefineData(raw);
        }

        public void UpdateTable(PhonemeTable table)
        {
            refined = table == null || raw.end_times == null ? null : table.RefineData(raw);
        }
    }
}
