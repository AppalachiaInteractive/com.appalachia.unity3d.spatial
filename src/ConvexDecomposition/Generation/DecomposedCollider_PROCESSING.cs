#if UNITY_EDITOR

#region

using Sirenix.OdinInspector;
using Unity.Profiling;

#endregion

namespace Appalachia.Spatial.ConvexDecomposition.Generation
{
    public partial class DecomposedCollider
    {
        private const string _PROC = "Processing";
        private const string _PROC_PROC = _PROC + "/Process";
        private const string _PROC_MESH = _PROC + "/Mesh";
        private const string _PROC_MORE = _PROC + "/More";
        private const string _PROC_LESS = _PROC+ "/Less";
        private const string _PROC_EQUA = _PROC + "/Equal";
        private const string _PROC_LEAS = _PROC + "/At Least";
        private const string _PROC_MOST = _PROC + "/At Most";
        private const string _PROC_PROC_BTN = _PROC_PROC + "/BTN";
        private const string _PROC_MESH_BTN = _PROC_MESH + "/BTN";
        private const string _PROC_MORE_BTN = _PROC_MORE + "/BTN";
        private const string _PROC_LESS_BTN = _PROC_LESS + "/BTN";
        private const string _PROC_EQUA_BTN = _PROC_EQUA + "/BTN";
        private const string _PROC_LEAS_BTN = _PROC_LEAS + "/BTN";
        private const string _PROC_MOST_BTN = _PROC_MOST + "/BTN";
     
        
        private bool _canProcess => (data != null) && (data.originalMesh != null) && !data.externallyCreated;
        private bool _canProcessMore => _canProcess /*&& decomposedMeshes.Length < 20*/;
        private bool _canProcessLess1 => _canProcess && (data.elements.Count > 1);
        private bool _canProcessLess2 => _canProcess && (data.elements.Count > 2);
        private bool _canProcessLess3 => _canProcess && (data.elements.Count > 3);
        private bool _canProcessLess5 => _canProcess && (data.elements.Count > 5);
        private bool _canProcessLess7 => _canProcess && (data.elements.Count > 7);
        private bool _canProcessLess10 => _canProcess && (data.elements.Count > 10);
        private bool _canProcessLess15 => _canProcess && (data.elements.Count > 15);
        private bool _canProcessLess20 => _canProcess && (data.elements.Count > 20);
        private bool _canProcessLess25 => _canProcess && (data.elements.Count > 25);
        private bool _canProcessLess30 => _canProcess && (data.elements.Count > 30);
        private bool _canProcessLess35 => _canProcess && (data.elements.Count > 35);
        private bool _canProcessLess40 => _canProcess && (data.elements.Count > 40);
        private bool _canProcessLess50 => _canProcess && (data.elements.Count > 50);
        private bool _canProcessLess60 => _canProcess && (data.elements.Count > 60);
        private bool _canProcessLess70 => _canProcess && (data.elements.Count > 70);
        private bool _canProcessLess80 => _canProcess && (data.elements.Count > 80);
        private bool _canProcessLess90 => _canProcess && (data.elements.Count > 90);
        private bool _canProcessLess100 => _canProcess && (data.elements.Count > 100);
        
        [BoxGroup(_PROC), PropertyOrder(50)]
        [HorizontalGroup(_PROC_PROC)]
        [ButtonGroup(_PROC_PROC_BTN), Button, EnableIf(nameof(_canProcess))]
        public void DecomposeMeshes()
        {
            ExecuteDecomposition(ExecutionStyle.Normal);
        }

        [ButtonGroup(_PROC_PROC_BTN), PropertyOrder(51), Button, EnableIf(nameof(_canProcess))]
        public void ForceDecompose()
        {
            ExecuteDecomposition(ExecutionStyle.Forced);
        }

        [ButtonGroup(_PROC_PROC_BTN), PropertyOrder(52), Button, EnableIf(nameof(_canProcess))]
        public void RebuildDecompose()
        {
            ExecuteDecomposition(ExecutionStyle.Rebuild);
        }
        

        private bool _canWorseMesh => _canProcess && (data != null) && (data.meshOffset > 0);
        private bool _canBetterMesh => _canProcess && (data != null) && (data.meshOffset < 100);

        [HorizontalGroup(_PROC_MESH)]
        [ButtonGroup(_PROC_MESH_BTN), PropertyOrder(55), Button, EnableIf(nameof(_canWorseMesh))]
        public void WorstMesh()
        {
            data.meshOffset = 0;
        }

        [ButtonGroup(_PROC_MESH_BTN), PropertyOrder(56), Button, EnableIf(nameof(_canWorseMesh))]
        public void WorseMesh()
        {
            data.meshOffset -= 1;
        }

        [ButtonGroup(_PROC_MESH_BTN), PropertyOrder(57), Button, EnableIf(nameof(_canBetterMesh))]
        public void BetterMesh()
        {
            data.meshOffset += 1;
        }

        [ButtonGroup(_PROC_MESH_BTN), PropertyOrder(58), Button, EnableIf(nameof(_canBetterMesh))]
        public void BestMesh()
        {
            data.meshOffset = 100;
        }
        
        [HorizontalGroup(_PROC_MOST)]
        [ButtonGroup(_PROC_MOST_BTN), Button("<=1"), EnableIf(nameof(_canProcessMore))]
        public void AtMost1()
        {
            ExecuteDecompositionAtMost(1);
        }

        [ButtonGroup(_PROC_MOST_BTN), Button("<=2"), EnableIf(nameof(_canProcessMore))]
        public void AtMost2()
        {
            ExecuteDecompositionAtMost(2);
        }

        [ButtonGroup(_PROC_MOST_BTN), Button("<=3"), EnableIf(nameof(_canProcessMore))]
        public void AtMost3()
        {
            ExecuteDecompositionAtMost(3);
        }

        [ButtonGroup(_PROC_MOST_BTN), Button("<=4"), EnableIf(nameof(_canProcessMore))]
        public void AtMost4()
        {
            ExecuteDecompositionAtMost(4);
        }

        [ButtonGroup(_PROC_MOST_BTN), Button("<=5"), EnableIf(nameof(_canProcessMore))]
        public void AtMost5()
        {
            ExecuteDecompositionAtMost(5);
        }

        [ButtonGroup(_PROC_MOST_BTN), Button("<=7"), EnableIf(nameof(_canProcessMore))]
        public void AtMost7()
        {
            ExecuteDecompositionAtMost(7);
        }

        [ButtonGroup(_PROC_MOST_BTN), Button("<=10"), EnableIf(nameof(_canProcessMore))]
        public void AtMost10()
        {
            ExecuteDecompositionAtMost(10);
        }

        [ButtonGroup(_PROC_MOST_BTN), Button("<=15"), EnableIf(nameof(_canProcessMore))]
        public void AtMost15()
        {
            ExecuteDecompositionAtMost(15);
        }
        
        [ButtonGroup(_PROC_MOST_BTN), Button("<=20"), EnableIf(nameof(_canProcessMore))]
        public void AtMost20()
        {
            ExecuteDecompositionAtMost(20);
        }
        
        [ButtonGroup(_PROC_MOST_BTN), Button("<=25"), EnableIf(nameof(_canProcessMore))]
        public void AtMost25()
        {
            ExecuteDecompositionAtMost(25);
        }

        [ButtonGroup(_PROC_MOST_BTN), Button("<=30"), EnableIf(nameof(_canProcessMore))]
        public void AtMost30()
        {
            ExecuteDecompositionAtMost(30);
        }
        
        [ButtonGroup(_PROC_MOST_BTN), Button("<=35"), EnableIf(nameof(_canProcessMore))]
        public void AtMost35()
        {
            ExecuteDecompositionAtMost(35);
        }
        
        [ButtonGroup(_PROC_MOST_BTN), Button("<=40"), EnableIf(nameof(_canProcessMore))]
        public void AtMost40()
        {
            ExecuteDecompositionAtMost(40);
        }
        
        [ButtonGroup(_PROC_MOST_BTN), Button("<=50"), EnableIf(nameof(_canProcessMore))]
        public void AtMost50()
        {
            ExecuteDecompositionAtMost(50);
        }
        
        /*
        [ButtonGroup(_PROC_MOST_BTN), Button("<=60"), EnableIf(nameof(_canProcessMore))]
        public void AtMost60()
        {
            ExecuteDecompositionAtMost(60);
        }
        
        [ButtonGroup(_PROC_MOST_BTN), Button("<=70"), EnableIf(nameof(_canProcessMore))]
        public void AtMost70()
        {
            ExecuteDecompositionAtMost(70);
        }
        
        [ButtonGroup(_PROC_MOST_BTN), Button("<=80"), EnableIf(nameof(_canProcessMore))]
        public void AtMost80()
        {
            ExecuteDecompositionAtMost(80);
        }*/
        
        [HorizontalGroup(_PROC_LESS)]
        [ButtonGroup(_PROC_LESS_BTN), Button("-1"), EnableIf(nameof(_canProcessLess1))]
        public void LessComplex1()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -1);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-2"), EnableIf(nameof(_canProcessLess2))]
        public void LessComplex2()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -2);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-3"), EnableIf(nameof(_canProcessLess3))]
        public void LessComplex3()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -3);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-4"), EnableIf(nameof(_canProcessLess3))]
        public void LessComplex4()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -4);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-5"), EnableIf(nameof(_canProcessLess5))]
        public void LessComplex5()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -5);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-7"), EnableIf(nameof(_canProcessLess7))]
        public void LessComplex7()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -7);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-10"), EnableIf(nameof(_canProcessLess10))]
        public void LessComplex10()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -10);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-15"), EnableIf(nameof(_canProcessLess15))]
        public void LessComplex15()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -15);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-20"), EnableIf(nameof(_canProcessLess20))]
        public void LessComplex20()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -20);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-25"), EnableIf(nameof(_canProcessLess25))]
        public void LessComplex25()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -25);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-30"), EnableIf(nameof(_canProcessLess30))]
        public void LessComplex30()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -30);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-35"), EnableIf(nameof(_canProcessLess35))]
        public void LessComplex35()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -35);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-40"), EnableIf(nameof(_canProcessLess40))]
        public void LessComplex40()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -40);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-50"), EnableIf(nameof(_canProcessLess50))]
        public void LessComplex50()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -50);
        }

        /*
        [ButtonGroup(_PROC_LESS_BTN), Button("-60"), EnableIf(nameof(_canProcessLess60))]
        public void LessComplex60()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -60);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-70"), EnableIf(nameof(_canProcessLess70))]
        public void LessComplex70()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -70);
        }

        [ButtonGroup(_PROC_LESS_BTN), Button("-80"), EnableIf(nameof(_canProcessLess80))]
        public void LessComplex80()
        {
            ExecuteDecompositionExplicit(data.elements.Count, -80);
        }
        */

        
        [HorizontalGroup(_PROC_EQUA)]
        [ButtonGroup(_PROC_EQUA_BTN), Button("=1"), EnableIf(nameof(_canProcess))]
        public void Exactly1()
        {
            ExecuteDecompositionExplicit(1, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=2"), EnableIf(nameof(_canProcess))]
        public void Exactly2()
        {
            ExecuteDecompositionExplicit(2, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=3"), EnableIf(nameof(_canProcess))]
        public void Exactly3()
        {
            ExecuteDecompositionExplicit(3, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=4"), EnableIf(nameof(_canProcess))]
        public void Exactly4()
        {
            ExecuteDecompositionExplicit(4, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=5"), EnableIf(nameof(_canProcess))]
        public void Exactly5()
        {
            ExecuteDecompositionExplicit(5, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=7"), EnableIf(nameof(_canProcess))]
        public void Exactly7()
        {
            ExecuteDecompositionExplicit(7, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=10"), EnableIf(nameof(_canProcess))]
        public void Exactly10()
        {
            ExecuteDecompositionExplicit(10, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=15"), EnableIf(nameof(_canProcess))]
        public void Exactly15()
        {
            ExecuteDecompositionExplicit(15, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=20"), EnableIf(nameof(_canProcess))]
        public void Exactly20()
        {
            ExecuteDecompositionExplicit(20, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=25"), EnableIf(nameof(_canProcess))]
        public void Exactly25()
        {
            ExecuteDecompositionExplicit(25, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=30"), EnableIf(nameof(_canProcess))]
        public void Exactly30()
        {
            ExecuteDecompositionExplicit(30, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=35"), EnableIf(nameof(_canProcess))]
        public void Exactly35()
        {
            ExecuteDecompositionExplicit(35, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=40"), EnableIf(nameof(_canProcess))]
        public void Exactly40()
        {
            ExecuteDecompositionExplicit(40, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=50"), EnableIf(nameof(_canProcess))]
        public void Exactly50()
        {
            ExecuteDecompositionExplicit(50, 0);
        }

        /*
        [ButtonGroup(_PROC_EQUA_BTN), Button("=60"), EnableIf(nameof(_canProcess))]
        public void Exactly60()
        {
            ExecuteDecompositionExplicit(60, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=70"), EnableIf(nameof(_canProcess))]
        public void Exactly70()
        {
            ExecuteDecompositionExplicit(70, 0);
        }

        [ButtonGroup(_PROC_EQUA_BTN), Button("=80"), EnableIf(nameof(_canProcess))]
        public void Exactly80()
        {
            ExecuteDecompositionExplicit(80, 0);
        }
        */
        
        [HorizontalGroup(_PROC_MORE)]
        [ButtonGroup(_PROC_MORE_BTN), Button("+1"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex1()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 1);
        }

        [ButtonGroup(_PROC_MORE_BTN), Button("+2"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex2()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 2);
        }

        [ButtonGroup(_PROC_MORE_BTN), Button("+3"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex3()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 3);
        }

        [ButtonGroup(_PROC_MORE_BTN), Button("+4"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex4()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 4);
        }

        [ButtonGroup(_PROC_MORE_BTN), Button("+5"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex5()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 5);
        }

        [ButtonGroup(_PROC_MORE_BTN), Button("+7"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex7()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 7);
        }

        [ButtonGroup(_PROC_MORE_BTN), Button("+10"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex10()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 10);
        }

        [ButtonGroup(_PROC_MORE_BTN), Button("+15"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex15()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 15);
        }
        
        [ButtonGroup(_PROC_MORE_BTN), Button("+20"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex20()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 20);
        }
        
        [ButtonGroup(_PROC_MORE_BTN), Button("+25"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex25()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 25);
        }

        [ButtonGroup(_PROC_MORE_BTN), Button("+30"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex30()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 30);
        }
        
        [ButtonGroup(_PROC_MORE_BTN), Button("+35"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex35()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 35);
        }
        
        [ButtonGroup(_PROC_MORE_BTN), Button("+40"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex40()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 40);
        }
        
        [ButtonGroup(_PROC_MORE_BTN), Button("+50"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex50()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 50);
        }
        
        /*[ButtonGroup(_PROC_MORE_BTN), Button("+60"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex60()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 60);
        }
        
        [ButtonGroup(_PROC_MORE_BTN), Button("+70"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex70()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 70);
        }
        
        [ButtonGroup(_PROC_MORE_BTN), Button("+80"), EnableIf(nameof(_canProcessMore))]
        public void MoreComplex80()
        {
            ExecuteDecompositionExplicit(data.elements.Count, 80);
        }*/
        
        
        [HorizontalGroup(_PROC_LEAS)]
        [ButtonGroup(_PROC_LEAS_BTN), Button(">=1"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast1()
        {
            ExecuteDecompositionAtLeast(1);
        }

        [ButtonGroup(_PROC_LEAS_BTN), Button(">=2"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast2()
        {
            ExecuteDecompositionAtLeast(2);
        }

        [ButtonGroup(_PROC_LEAS_BTN), Button(">=3"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast3()
        {
            ExecuteDecompositionAtLeast(3);
        }

        [ButtonGroup(_PROC_LEAS_BTN), Button(">=4"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast4()
        {
            ExecuteDecompositionAtLeast(4);
        }

        [ButtonGroup(_PROC_LEAS_BTN), Button(">=5"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast5()
        {
            ExecuteDecompositionAtLeast(5);
        }

        [ButtonGroup(_PROC_LEAS_BTN), Button(">=7"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast7()
        {
            ExecuteDecompositionAtLeast(7);
        }

        [ButtonGroup(_PROC_LEAS_BTN), Button(">=10"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast10()
        {
            ExecuteDecompositionAtLeast(10);
        }

        [ButtonGroup(_PROC_LEAS_BTN), Button(">=15"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast15()
        {
            ExecuteDecompositionAtLeast(15);
        }
        
        [ButtonGroup(_PROC_LEAS_BTN), Button(">=20"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast20()
        {
            ExecuteDecompositionAtLeast(20);
        }
        
        [ButtonGroup(_PROC_LEAS_BTN), Button(">=25"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast25()
        {
            ExecuteDecompositionAtLeast(25);
        }

        [ButtonGroup(_PROC_LEAS_BTN), Button(">=30"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast30()
        {
            ExecuteDecompositionAtLeast(30);
        }
        
        [ButtonGroup(_PROC_LEAS_BTN), Button(">=35"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast35()
        {
            ExecuteDecompositionAtLeast(35);
        }
        
        [ButtonGroup(_PROC_LEAS_BTN), Button(">=40"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast40()
        {
            ExecuteDecompositionAtLeast(40);
        }
        
        [ButtonGroup(_PROC_LEAS_BTN), Button(">=50"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast50()
        {
            ExecuteDecompositionAtLeast(50);
        }
        
        /*
        [ButtonGroup(_PROC_LEAS_BTN), Button(">=60"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast60()
        {
            ExecuteDecompositionAtLeast(60);
        }
        
        [ButtonGroup(_PROC_LEAS_BTN), Button(">=70"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast70()
        {
            ExecuteDecompositionAtLeast(70);
        }
        
        [ButtonGroup(_PROC_LEAS_BTN), Button(">=80"), EnableIf(nameof(_canProcessMore))]
        public void AtLeast80()
        {
            ExecuteDecompositionAtLeast(80);
        }*/

        private enum ExecutionStyle
        {
            Normal                 = 000,
            Forced                 = 002,
            Rebuild                = 003,
        }
       
        private static readonly ProfilerMarker _PRF_ExecuteDecomposition = new ProfilerMarker(_PRF_PFX + nameof(ExecuteDecomposition));

        private void ExecuteDecomposition(ExecutionStyle style)
        {
            using (_PRF_ExecuteDecomposition.Auto())
            {
                if (data == null) return;
                
                ExecuteDecompositionExplicit(
                    style == ExecutionStyle.Rebuild
                        ? data.elements.Count
                        : 0,
                    0,
                    style == ExecutionStyle.Normal
                );
            }
        }

        private void ExecuteDecompositionExplicit(int leveragedParts, int modifications, bool normal = false)
        {
            data.ExecuteDecompositionExplicit(gameObject, leveragedParts, modifications, normal);
        }
        
        private void ExecuteDecompositionAtLeast(int threshold)
        {
            if ((data.elements.Count != 0) && (data.elements.Count >= threshold))
            {
                return;
            }

            data.ExecuteDecompositionExplicit(gameObject, threshold, 0);
        }
        
        private void ExecuteDecompositionAtMost(int threshold)
        {
            if ((data.elements.Count != 0) && (data.elements.Count <= threshold))
            {
                return;
            }

            data.ExecuteDecompositionExplicit(gameObject, threshold, 0);
        }
    }
}

#endif