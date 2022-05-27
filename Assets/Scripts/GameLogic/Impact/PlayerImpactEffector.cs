using UnityEngine;

namespace com.louis.shootball
{
    public class PlayerImpactEffector : MonoBehaviour, IImpactEffector
    {
        public float mass = 1.0f; // defines the character mass
        public Vector3 impact = Vector3.zero;
        CharacterController character;
        FootballThirdPersonController thirdPersonController;
        // Start is called before the first frame update
        void Start()
        {
            character = GetComponent<CharacterController>();
            thirdPersonController = GetComponent<FootballThirdPersonController>();
        }

        public void AddImpact(Vector3 dir, float force, object source)
        {
            dir.Normalize();
            if (dir.y < 0) dir.y = -dir.y; // reflect down force on the ground
            impact += dir.normalized * force / mass;
        }

        // Update is called once per frame
        void Update()
        {
            // apply the impact force:
            if (impact.magnitude > 0.2)
            {
                character.Move(impact * Time.deltaTime);
            }
            // consumes the impact energy each cycle:
            impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
        }
    }
}