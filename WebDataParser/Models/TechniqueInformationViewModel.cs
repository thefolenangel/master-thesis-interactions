using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DataSetGenerator;
using Newtonsoft.Json;

namespace WebDataParser.Models {
    public class TechniqueInformationViewModel {

        public float[][] PushTime { get; set; }
        public float[][] PushHitRate { get; set; }
        public float[][] PushAccuracy { get; set; }

        public float[][] PullTime { get; set; }
        public float[][] PullHitRate { get; set; }
        public float[][] PullAccuracy { get; set; }

        public int TotalUsers { get; set; }
        public int TotalAttempts { get; set; }

        public TechniqueInformationViewModel(IEnumerable<Attempt> attempts, int count) {

            List<Attempt> pushAttempts = (from attempt in attempts
                                                where attempt.Direction == GestureDirection.Push
                                                select attempt).ToList();
            List<Attempt> pullAttempts = (from attempt in attempts
                                                where attempt.Direction == GestureDirection.Pull
                                                select attempt).ToList();

            TotalUsers = count;
            TotalAttempts = pushAttempts.Count;

            PullTime = GetTimeInformation(pullAttempts);
            PushTime = GetTimeInformation(pushAttempts);

            PullHitRate = GetHitRateInformation(pullAttempts); 
            PushHitRate = GetHitRateInformation(pushAttempts);

            PullAccuracy = GetAccuracyInformation(pullAttempts);
            PushAccuracy = GetAccuracyInformation(pushAttempts);

        }

        private float[][] GetHitRateInformation(List<Attempt> attempts) {

            //HPM = (float)attempts.Sum(attemtp => attemtp.Hit ? 1 : 0) / (float)attempts.Count;
            //HPSTD = (float)Math.Sqrt(attempts.Sum(attempt => Math.Pow((attempt.Hit ? 1 : 0) - HPM, 2)) / attempts.Count);

            float[][] hitRateInfo = new float[4][];
            foreach (var technique in DataGenerator.AllTechniques) {
                var techAttempts = attempts.Where(attempt => attempt.Type == technique);

                float tNum = (float)technique + 1;

                float tMean = (float)techAttempts.Sum(attemtp => attemtp.Hit ? 1 : 0) / techAttempts.Count();
                if (float.IsNaN(tMean)) tMean = 0;
                
                float tStd = (float)Math.Sqrt(techAttempts.Sum(attempt => Math.Pow((attempt.Hit ? 1 : 0) - tMean, 2)) / techAttempts.Count());
                if (float.IsNaN(tStd)) tStd = 0;

                hitRateInfo[(int)technique] = new float[] { tNum, tMean, tStd };
            }
            return hitRateInfo;

        }
        

        private float[][] GetAccuracyInformation(List<Attempt> attempts) {


            //ACCM = (float)attempts.Sum(attempt => attempt.Hit ? 0 : MathHelper.DistanceToTargetCell(attempt)) / (float)attempts.Count;
            //ACCSTD = (float)Math.Sqrt(attempts.Sum(attempt => Math.Pow(MathHelper.DistanceToTargetCell(attempt) - ACCM, 2)) / attempts.Count);

            float[][] accuracyInfo = new float[4][];
            foreach (var technique in DataGenerator.AllTechniques) {
                var techAttempts = attempts.Where(attempt => attempt.Type == technique);

                float tNum = (float)technique + 1;

                float tMean = (float)techAttempts.Sum(attempt => attempt.Hit ? 0 : MathHelper.DistanceToTargetCell(attempt)) / techAttempts.Count();
                if (float.IsNaN(tMean)) tMean = 0;

                float tStd = (float)Math.Sqrt(techAttempts.Sum(attempt => Math.Pow(MathHelper.DistanceToTargetCell(attempt) - tMean, 2)) / techAttempts.Count());
                if (float.IsNaN(tStd)) tStd = 0;

                accuracyInfo[(int)technique] = new float[] { tNum, tMean, tStd };
            }
            return accuracyInfo;
        }

        private float[][] GetTimeInformation(List<Attempt> attempts) {

            //TTM = (float)attempts.Sum(attempt => attempt.Time.TotalSeconds) / (float)attempts.Count;
            //TTSTD = (float)Math.Sqrt(attempts.Sum(attempt => Math.Pow(attempt.Time.TotalSeconds - TTM, 2)) / attempts.Count);

            float[][] timeInfo = new float[4][];
            foreach(var technique in DataGenerator.AllTechniques) {
                var techAttempts = attempts.Where(attempt => attempt.Type == technique);

                float tNum = (float)technique + 1;

                float tMean = (float)techAttempts.Sum(attempt => attempt.Time.TotalSeconds) / techAttempts.Count();
                if (float.IsNaN(tMean)) tMean = 0;

                float tStd = (float)Math.Sqrt(techAttempts.Sum(attempt => Math.Pow(attempt.Time.TotalSeconds - tMean, 2)) / techAttempts.Count());
                if (float.IsNaN(tStd)) tStd = 0;

                timeInfo[(int)technique] = new float[]{ tNum, tMean, tStd };
            }
            return timeInfo;
        }
    }
}