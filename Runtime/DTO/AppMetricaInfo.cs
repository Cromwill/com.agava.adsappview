using UnityEngine;

namespace AdsAppView.DTO
{
    [CreateAssetMenu(fileName = "AppMetricaInfo", menuName = "Create AppMetricaInfo/AdsAppView")]
    public class AppMetricaInfo : ScriptableObject
    {
        [field: SerializeField] public string Key { get; private set; }
        [field: SerializeField] public string Id { get; private set; }
    }
}
