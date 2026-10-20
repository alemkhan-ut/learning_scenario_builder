using UnityEngine;

namespace LearningScenarioBuilder.Steps
{
    [RequireComponent(typeof(Collider2D))]
    public class TriggerEventSource : TaggedEventSource
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            RaiseIfTagMatches(other.gameObject);
        }
    }
}
