using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resemble
{
    [System.Serializable]
    public class PhonemeData
    {

        public PhonemesRaw raw;
        public Phonemes refined;

        public PhonemeData(PhonemesRaw raw, PhonemeTable table)
        {
            this.raw = raw;
            refined = table == null ? null : table.RefineData(raw);
        }

        public void SetData(PhonemesRaw raw, PhonemeTable table)
        {
            this.raw = raw;
            refined = table == null ? null : table.RefineData(raw);
        }

        public void UpdateTable(PhonemeTable table)
        {
            refined = table == null ? null : table.RefineData(raw);
        }
    }
}
