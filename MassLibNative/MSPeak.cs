using System;
using System.Collections.Generic;
using System.Text;

namespace COL.MassLib
{
    [Serializable]
    public class MSPeak : IComparable<MSPeak>
    {
        private float _monoMass; //First peak
        private float _monoIntemsity;
        private float _chargeState =1.0f;
        private float _deisotopeMz;
        private float _fitScore;
        private float _mostIntseMass;
        private double _clusterIntensity;
        private double _mostIntseIntensity;
        /// <summary>
        /// Mass Scan
        /// </summary>
        /// <param name="argMonoMass"></param>
        /// <param name="argMonoIntensity"></param>
        /// <param name="argChargeState"></param>
        /// <param name="argDeisotopeMz"></param>
        /// <param name="argFixScore"></param>
        /// <param name="argMostIntenseMass"></param>
        public MSPeak(float argMonoMass, float argMonoIntensity, float argChargeState, float argDeisotopeMz, float argFixScore, float argMostIntenseMass, double argMostIntenseIntensity)
        {
            _monoMass = argMonoMass;
            _monoIntemsity = argMonoIntensity;
            _chargeState = argChargeState;
            _deisotopeMz = argDeisotopeMz;
            _fitScore = argFixScore;
            _mostIntseMass = argMostIntenseMass;
            _mostIntseIntensity = argMostIntenseIntensity;
        }
        /// <summary>
        /// Mass Scan
        /// </summary>
        /// <param name="argMonoMass"></param>
        /// <param name="argMonoIntensity"></param>
        /// <param name="argChargeState"></param>
        /// <param name="argDeisotopeMz"></param>
        /// <param name="argFixScore"></param>
        /// <param name="argMostIntenseMass"></param>
        public MSPeak(float argMonoMass, float argMonoIntensity, float argChargeState, float argDeisotopeMz, float argFixScore, float argMostIntenseMass,double argMostIntenseIntensity, double argTotalClusterIntensity)
        {
            _monoMass = argMonoMass;
            _monoIntemsity = argMonoIntensity;
            _chargeState = argChargeState;
            _deisotopeMz = argDeisotopeMz;
            _fitScore = argFixScore;
            _mostIntseMass = argMostIntenseMass;
            _clusterIntensity = argTotalClusterIntensity;
            _mostIntseIntensity = argMostIntenseIntensity;
        }
        /// <summary>
        /// Tendens Mass Scan
        /// </summary>
        /// <param name="argMonoMass"></param>
        /// <param name="argMonoIntensity"></param>
        public MSPeak(float argMonoMass, float argMonoIntensity)
        {
            _monoMass = argMonoMass;
            _monoIntemsity = argMonoIntensity;
            _chargeState = 1.0f;
        }
        public double ClusterIntensity
        {
            get { return _clusterIntensity; }
        }
        public float MonoMass
        {
            get { return _monoMass; }
        }
        public float MonoIntensity
        {
            get { return _monoIntemsity; }
        }
        public float ChargeState
        {
            get { return _chargeState; }
        }
        public float DeisotopeMz
        {
            get { return _deisotopeMz; }
        }
        public float FitScore
        {
            get { return _fitScore; }
        }
        public float MostIntenseMass
        {
            get { return _mostIntseMass; }
        }
        public double MostIntenseIntensity
        {
            get { return _mostIntseIntensity; }
        }
        /// <summary>
        /// First peak of envelope
        /// </summary>
        public float MonoisotopicMZ
        {
            get { return Convert.ToSingle((_monoMass + _chargeState * MassLib.Atoms.ProtonMass) / _chargeState); }
        }
        public int CompareTo(MSPeak obj)
        {
            return this.MonoisotopicMZ.CompareTo(obj.MonoisotopicMZ); //low to high
        }
    }
}
