using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3
{
	internal class RigidPhysicalCharacteristics
	{
		public float Mass { get; private protected set; }
		public float CoefficientOfRestitution { get; private protected set; }
        public float CoefficientOfFriction { get; private protected set; }
        public float MovementDampingCoefficient { get; private protected set; }
        public float RotationDampingCoefficient { get; private protected set; }

        /// <param name="Mass">0にすると動かないstatic剛体になる。</param>
        public RigidPhysicalCharacteristics( float Mass = 0, float CoefficientOfRestitution = 0, float CoefficientOfFriction = 0.5f, float MovementDampingCoefficient = 0, float RotationDampingCoefficient = 0 )
		{
			this.Mass = Mass;
			this.CoefficientOfRestitution = CoefficientOfRestitution;
			this.CoefficientOfFriction = CoefficientOfFriction;
			this.MovementDampingCoefficient = MovementDampingCoefficient;
			this.RotationDampingCoefficient = RotationDampingCoefficient;
		}
	}
}
