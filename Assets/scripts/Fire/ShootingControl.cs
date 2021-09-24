using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

namespace Fire
{
    public class ShootingControl : NetworkBehaviour
    {
        private NetworkVariableBool _stopParticle = new NetworkVariableBool(new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly
        });

        [SerializeField] private float bulletForce;
        [SerializeField] private float shootingThreshold = 0.2f;

        private float _shotCounter;
        public Transform firePoint;
        private ParticleSystem _particleSystem;

        private float _timeToChangeAnimation;

        public GameObject bulletPrefab;

        private void Start()
        {
            _particleSystem = firePoint.GetComponentInChildren<ParticleSystem>();
            _particleSystem.Stop();
        }

        void Update()
        {
            if (IsLocalPlayer)
            {
                if (Input.GetButton("Fire1"))
                {
                    _shotCounter -= Time.deltaTime;
                    if (_shotCounter <= 0)
                    {
                        _shotCounter = shootingThreshold;
                        Shoot();
                    }
                }
                else
                {
                    _shotCounter = 0;
                }
            }

            if (_particleSystem.isPlaying)
            {
                _particleSystem.Stop();
            }

            if (!_stopParticle.Value)
            {
                _particleSystem.Play();
                if (IsLocalPlayer)
                {
                    ChangeParticleStateServerRpc(true);
                }
            }
        }

        // Update is called once per frame

        private void Shoot()
        {
            if (IsServer)
            {
                CreateBullet();
                PlayMuzzleFlashParticle();
            }
            else
            {
                SpawnBulletServerRpc();
                ChangeParticleStateServerRpc(false);
            }
        }

        [ServerRpc]
        public void SpawnBulletServerRpc()
        {
            CreateBullet();
        }

        [ServerRpc]
        public void ChangeParticleStateServerRpc(bool state)
        {
            _stopParticle.Value = state;
        }

        private void CreateBullet()
        {
            var bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            bullet.GetComponent<NetworkObject>().Spawn();
            var bulletRigidBody = bullet.GetComponent<Rigidbody>();
            bulletRigidBody.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
        }

        private void PlayMuzzleFlashParticle()
        {
            _stopParticle.Value = false;
        }
    }
}