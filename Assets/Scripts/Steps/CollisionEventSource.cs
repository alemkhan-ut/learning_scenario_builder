using UnityEngine;

namespace LearningScenarioBuilder.Steps
{
    [RequireComponent(typeof(Collider2D))]
    public class CollisionEventSource : TaggedEventSource
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            RaiseIfTagMatches(collision.gameObject);
        }
    }
}
