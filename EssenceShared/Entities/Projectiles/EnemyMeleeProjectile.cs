﻿using EssenceShared.Entities.Players;

namespace EssenceShared.Entities.Projectiles {
    public class EnemyMeleeProjectile : Projectile {
        public EnemyMeleeProjectile(int damage, string url, string id)
            : base(url, id) {
            Scale = Settings.Scale;
            Tag = Tags.EnemyProjectile;
            AttackDamage = damage;
            Speed = 50;
            DieAfter = 0.1f;
        }

        public override void Update(float dt) {
            base.Update(dt);
            MoveByAngle(Direction, Speed*dt);
        }

        public override void Collision(Entity other) {
            base.Collision(other);


            if (other.Tag == Tags.Player) {
                if (other as Player != null && !AlreadyDamaged.Contains(other)) {
                    (other as Player).Hp.Current -= AttackDamage;
                    AlreadyDamaged.Add(other);
                }
            }
        }
    }
}