using UnityEngine;

namespace AdsAppView.DTO
{
    [CreateAssetMenu(fileName = "AppMetricaInfo", menuName = "Create AppMetricaInfo", order = 51)]
    public class AppMetricaInfo : ScriptableObject
    {
        [field: SerializeField] public string Key { get; private set; }
    }
}
