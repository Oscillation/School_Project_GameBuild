﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GameBuild
{
    public class cCharacter
    {
        public bool up = false, right = false, down = true, left = false;
        public bool faceUp = false, faceDown = false, faceLeft = false, faceRight = false;
        public bool attacking = false;
        public bool inCombat;
        public bool showInventory;
        public bool dead;
        public bool cutscene;
        bool isHit = false;

        public string gender;

        public int playerHeight = 48; //size of the player sprite in pixels
        public int playerWidth = 48;
        public int damage;
        public int speed;

        public float regenAmount;

        public Rectangle position;
        public Rectangle interactRect; //for npcs and stuff
        Rectangle colRect;
        public Vector2 vectorPos; //for camera
        public Rectangle attackRectangle;
        public Rectangle warpRectangle;
        Rectangle healthPos;
        public Vector2 bossTarget;//when the boss shoots.. follows the player with less speed...

        public Texture2D spriteWalkSheet;
        public Texture2D spriteAttackSheet;
        public Texture2D shadowBlob;
        Texture2D debugTexture;
        Texture2D healthTexture;
        Texture2D healthBar;

        H_Map.TileMap tile;
        Random rand = new Random();

        public Color color = new Color(255, 255, 255, 255); //blink when player gets hit
        public Color hpColor;

        public float health;
        float healthBarWidth;
        float healthPct;
        public float maxHealth;
        float regenTimer = 1;
        const float REGENTIMER = 1;
        float bleedTimer = 3;
        const float BLEEDTIMER = 3;
        const float ATTACKTIMER = 500f; //in total milliseconds
        float attackTimer = 500f;
        public float targetSpeed = 4;//for boss target

        public Inventory inventory;
        public List<DamageEffect> damageEffectList = new List<DamageEffect>();

        AnimationComponent animation;

        ParticleSystemEmitter emitter;

        //Animations
        const int WALK_UP = 0;
        const int WALK_RIGHT = 1;
        const int WALK_DOWN = 2;
        const int WALK_LEFT = 3;
        bool walking = false;

        public cCharacter(Game1 game, string gender)
        {
            health = 100;
            speed = 4;
            maxHealth = health;
            healthBar = game.Content.Load<Texture2D>(@"Game\Hp bar");
            this.gender = gender;

            #region Textures
            debugTexture = game.Content.Load<Texture2D>(@"Game\blackness");
            spriteWalkSheet = game.Content.Load<Texture2D>("player/CharaWalkSheet");
            spriteAttackSheet = game.Content.Load<Texture2D>("player/CharaAttackSheet V2");
            shadowBlob = game.Content.Load<Texture2D>("player/shadowTex");
            healthTexture = game.Content.Load<Texture2D>(@"Game\health100");
            #endregion

            #region Rectangles and Vectors
            position = new Rectangle(640, 640, playerWidth, playerWidth);
            interactRect = new Rectangle(position.X - (position.Width / 2), position.Y - (position.Height / 2), position.Width * 2, position.Height * 2);
            vectorPos = new Vector2(position.X, position.Y);
            colRect = new Rectangle();
            attackRectangle = new Rectangle();
            warpRectangle = new Rectangle();
            healthPos = new Rectangle();
            #endregion

            hpColor = new Color(200, 200, 200, 255);

            inventory = new Inventory(game);
            animation = new AnimationComponent(3, 4, 72, 96, 100, Point.Zero);
            emitter = new ParticleSystemEmitter(game);
            Game1.particleSystem.emitters.Add(emitter);
            bossTarget = new Vector2(position.X, position.Y);
        }

        public void Update(Game1 game, H_Map.TileMap tiles, GameTime gameTime, KeyboardState oldState, GraphicsDevice graphicsDevice)
        {
            #region Things to update every frame, positions and stuff
            healthPct = (health / maxHealth);
            healthBarWidth = (float)healthTexture.Width * healthPct;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (showInventory)
            {
                animation.PauseAnimation();
            }

            //if (Game1.testBoss.currentPhase != Npc.Boss.phase.sleep)
            {
                if (position.X + (position.Width / 2) - 16 > bossTarget.X)
                {
                    bossTarget.X += targetSpeed / 1.5f;
                }
                if (position.X + (position.Width / 2) - 16 < bossTarget.X)
                {
                    bossTarget.X -= targetSpeed / 1.5f;
                }
                if (position.Y + (position.Width / 2) - 16 > bossTarget.Y)
                {
                    bossTarget.Y += targetSpeed / 1.5f;
                }
                if (position.Y + (position.Width / 2) - 16 < bossTarget.Y)
                {
                    bossTarget.Y -= targetSpeed / 1.5f;
                }
            }

            if (isHit)
            {
                Bleed();
                bleedTimer -= elapsed;
            }
            if (bleedTimer <= 0)
            {
                isHit = false;
                bleedTimer = BLEEDTIMER;
            }

            if (health <= 0)
            {
                dead = true;
                DeathEffect(game);
            }

            if (inCombat)
            {
                regenTimer = REGENTIMER;
            }
            else
                regenTimer -= elapsed;

            if (regenTimer <= 0)
            {
                if (health < maxHealth && !inCombat && !dead)
                {
                    regenAmount = rand.Next(3, 7);

                    if (maxHealth - health < regenAmount)
                        regenAmount = maxHealth - health;

                    health += regenAmount;
                    damageEffectList.Add(new DamageEffect((int)regenAmount, game, new Vector2(position.X, position.Y - 16), new Color(0, 255, 0, 255), "regen"));
                    Regen((int)regenAmount);
                }

                regenTimer = REGENTIMER;
            }
            tile = tiles;
            interactRect.X = position.X - (position.Width / 2);
            interactRect.Y = position.Y - (position.Height / 2);
            Rectangle location = new Rectangle(position.X, position.Y, position.Width, position.Height);
            Rectangle corner1 = new Rectangle(colRect.X, colRect.Y, colRect.Width, colRect.Height);
            Rectangle corner2 = new Rectangle(colRect.X, colRect.Y, colRect.Width, colRect.Height);
            Rectangle halfcorner1 = new Rectangle(colRect.X, colRect.Y, colRect.Width, colRect.Height);
            Rectangle halfcorner2 = new Rectangle(colRect.X, colRect.Y + halfcorner1.Height, colRect.Width, colRect.Height);
            healthPos.X = position.X;
            healthPos.Y = position.Y - 10;
            healthPos.Width = (int)healthBarWidth;
            healthPos.Height = healthTexture.Height;
            vectorPos.X = position.X;
            vectorPos.Y = position.Y;
            if (left)
            {
                attackRectangle.Width = position.Width * 2;
                attackRectangle.Height = 54;
                attackRectangle.X = position.X - (attackRectangle.Width / 2) + 12;
                attackRectangle.Y = position.Y + 5;

                warpRectangle.Width = position.Width / 2;
                warpRectangle.Height = 5;
                warpRectangle.X = position.X - warpRectangle.Width;
                warpRectangle.Y = position.Y + (position.Height / 2) + 2;
            }
            if (right)
            {
                attackRectangle.Width = position.Width * 2;
                attackRectangle.Height = 54;
                attackRectangle.X = position.X;
                attackRectangle.Y = position.Y + 5;

                warpRectangle.Width = position.Width / 2;
                warpRectangle.Height = 5;
                warpRectangle.X = position.X + position.Width;
                warpRectangle.Y = position.Y + (position.Height / 2) + 2;
            }
            if (up)
            {
                attackRectangle.Width = 54;
                attackRectangle.Height = position.Height * 2;
                attackRectangle.X = position.X;
                attackRectangle.Y = position.Y - (position.Height / 2);

                warpRectangle.Width = 5;
                warpRectangle.Height = position.Height / 2;
                warpRectangle.X = position.X + (position.Width / 2) + 2;
                warpRectangle.Y = position.Y - warpRectangle.Height;
            }
            if (down)
            {
                attackRectangle.Width = 54;
                attackRectangle.Height = position.Height * 2;
                attackRectangle.X = position.X;
                attackRectangle.Y = position.Y;

                warpRectangle.Width = 5;
                warpRectangle.Height = position.Height / 2;
                warpRectangle.X = position.X + (position.Width / 2);
                warpRectangle.Y = position.Y + position.Height;
            }

            animation.UpdateAnimation(gameTime);
            #endregion

            #region walk
            if (up)
            {
                faceUp = true;
                faceRight = false;
                faceLeft = false;
                faceDown = false;
            }
            if (down)
            {
                faceDown = true;
                faceRight = false;
                faceLeft = false;
                faceUp = false;
            }
            if (left)
            {
                faceLeft = true;
                faceRight = false;
                faceUp = false;
                faceDown = false;
            }
            if (right)
            {
                faceRight = true;
                faceUp = false;
                faceLeft = false;
                faceDown = false;
            }

            if (!attacking && !showInventory && !dead)// && cWarp.canWalk
            {
                walking = false;
                if (game.keyState.IsKeyDown(Keys.Up))
                {
                    //effects
                    if (Game1.map.backgroundLayer[position.X / Game1.map.tileWidth, position.Y / Game1.map.tileHeight].tileID == 4
                        || Game1.map.backgroundLayer[position.X / Game1.map.tileWidth, position.Y / Game1.map.tileHeight].tileID == 8)
                    {
                        Splash();
                    }
                    else
                        Walk();

                    up = true;
                    down = false;
                    left = false;
                    right = false;
                    location.Y -= speed;
                    corner1 = tile.GetTileRectangleFromPosition(location.X, location.Y + (position.Height / 2));
                    corner2 = tile.GetTileRectangleFromPosition(location.X + playerWidth, location.Y + (position.Height / 2));
                    halfcorner1 = tile.GetTileRectangleFromPosition(location.X, location.Y);
                    halfcorner2 = tile.GetTileRectangleFromPosition(location.X, location.Y);
                    if (!animation.IsAnimationPlaying(WALK_UP))
                    {
                        animation.LoopAnimation(WALK_UP);
                    }
                    walking = true;
                }
                else if (game.keyState.IsKeyDown(Keys.Down))
                {
                    //effects
                    if (Game1.map.backgroundLayer[position.X / Game1.map.tileWidth, position.Y / Game1.map.tileHeight].tileID == 4
                        || Game1.map.backgroundLayer[position.X / Game1.map.tileWidth, position.Y / Game1.map.tileHeight].tileID == 8)
                    {
                        Splash();
                    }
                    else
                        Walk();

                    down = true;
                    up = false;
                    right = false;
                    left = false;
                    location.Y += speed;
                    corner1 = tile.GetTileRectangleFromPosition(location.X, location.Y + playerHeight);
                    corner2 = tile.GetTileRectangleFromPosition(location.X + playerWidth, location.Y + playerHeight);
                    if (!animation.IsAnimationPlaying(WALK_DOWN))
                    {
                        animation.LoopAnimation(WALK_DOWN);
                    }
                    walking = true;
                }

                if (!IsCollision(tiles, corner1) && !IsCollision(tiles, corner2))
                {
                    position.Y = location.Y;
                    colRect = position;
                }

                if (game.keyState.IsKeyDown(Keys.Right))
                {
                    //effects
                    if (Game1.map.backgroundLayer[position.X / Game1.map.tileWidth, position.Y / Game1.map.tileHeight].tileID == 4
                        || Game1.map.backgroundLayer[position.X / Game1.map.tileWidth, position.Y / Game1.map.tileHeight].tileID == 8)
                    {
                        Splash();
                    }
                    else
                        Walk();

                    right = true;
                    left = false;
                    up = false;
                    down = false;
                    location.Y = position.Y;
                    location.X += speed;
                    corner1 = tile.GetTileRectangleFromPosition(location.X + playerWidth, location.Y + (position.Height / 2));
                    corner2 = tile.GetTileRectangleFromPosition(location.X + playerWidth, location.Y + playerHeight);
                    if (!animation.IsAnimationPlaying(WALK_RIGHT))
                    {
                        animation.LoopAnimation(WALK_RIGHT);
                    }
                    walking = true;
                }
                else if (game.keyState.IsKeyDown(Keys.Left))
                {
                    //effects
                    if (Game1.map.backgroundLayer[position.X / Game1.map.tileWidth, position.Y / Game1.map.tileHeight].tileID == 4
                        || Game1.map.backgroundLayer[position.X / Game1.map.tileWidth, position.Y / Game1.map.tileHeight].tileID == 8)
                    {
                        Splash();
                    }
                    else
                        Walk();

                    left = true;
                    right = false;
                    up = false;
                    down = false;
                    location.X -= speed;
                    location.Y = position.Y;
                    corner1 = tile.GetTileRectangleFromPosition(location.X, location.Y + (position.Height / 2));
                    corner2 = tile.GetTileRectangleFromPosition(location.X, location.Y + playerHeight);
                    if (!animation.IsAnimationPlaying(WALK_LEFT))
                    {
                        animation.LoopAnimation(WALK_LEFT);
                    }
                    walking = true;
                }

                if (!walking)
                {
                    animation.PauseAnimation();
                }

                if (!IsCollision(tiles, corner1) && !IsCollision(tiles, corner2))
                {
                    position.X = location.X;
                    colRect = position;
                }
            }


            #endregion

            if (game.keyState.IsKeyDown(Keys.Tab) && oldState.IsKeyUp(Keys.Tab) && !dead)
            {
                if (showInventory)
                {
                    showInventory = false;
                }
                else
                    showInventory = true;
            }

            #region Attack
            attackTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (game.keyState.IsKeyDown(Keys.Z) && game.oldState.IsKeyUp(Keys.Z) && !dead && attackTimer <= 0)
            {
                damage = game.damageObject.dealDamage(5, 30);
                attackTimer = ATTACKTIMER;
                foreach (Npc.Npc npc in game.activeNpcs)
                {
                    if (npc.position.Intersects(attackRectangle))
                    {
                        if (npc.health > 0 && npc.IsOnMap())
                        {
                            damageEffectList.Add(new DamageEffect(damage, game, new Vector2(npc.position.X, npc.position.Y - 16), new Color(255, 255, 255, 255), "player"));
                            npc.health -= damage;
                            npc.attackPlayer = true;
                        }
                    }
                }
                if (position.Intersects(Game1.testBoss.position))
                {
                    if (Game1.testBoss.health > 0 && Game1.testBoss.IsOnMap())
                    {
                        damageEffectList.Add(new DamageEffect(damage, game, new Vector2(Game1.testBoss.position.X, Game1.testBoss.position.Y - 16), new Color(255, 255, 255, 255), "player"));
                        Game1.testBoss.health -= damage;
                        Game1.testBoss.currentPhase = Npc.Boss.phase.sleep;
                    }
                }
                for (int i = 0; i < Game1.testBoss.mobs.Count; i++)
                {
                    if (position.Intersects(Game1.testBoss.mobs[i].position))
                    {
                        if (Game1.testBoss.mobs[i].health > 0)
                        {
                            damageEffectList.Add(new DamageEffect(damage, game, new Vector2(Game1.testBoss.mobs[i].position.X, Game1.testBoss.mobs[i].position.Y - 16), new Color(255, 255, 255, 255), "player"));
                            Game1.testBoss.mobs[i].health -= damage;
                        }
                    }
                }
                for (int i = 0; i < game.Mobs.Count; i++)
                {
                    if (game.Mobs[i].position.Intersects(attackRectangle))
                    {
                        if (game.Mobs[i].health > 0 && game.Mobs[i].IsOnMap())
                        {
                            damageEffectList.Add(new DamageEffect(damage, game, new Vector2(game.Mobs[i].position.X, game.Mobs[i].position.Y - 16), new Color(255, 255, 255, 255), "player"));
                            game.Mobs[i].health -= damage;
                            game.Mobs[i].attackPlayer = true;
                        }
                    }
                }
            }

            for (int i = 0; i < damageEffectList.Count; i++)
            {
                damageEffectList[i].Effect();
            }

            for (int i = 0; i < game.Npcs.Count; i++)
            {
                if (game.Npcs[i].health > 0 && game.Npcs[i].IsOnMap() && game.Npcs[i].vulnerable)
                {
                    if (game.Npcs[i].combatRectangle.Intersects(position))
                    {
                        inCombat = true;
                    }
                    else
                        inCombat = false;
                }
                else
                {
                    inCombat = false;
                }
            }
            for (int i = 0; i < game.Mobs.Count; i++)
            {
                if (game.Mobs[i].health > 0 && game.Mobs[i].IsOnMap() && game.Mobs[i].vulnerable)
                {
                    if (game.Mobs[i].combatRectangle.Intersects(position))
                    {
                        inCombat = true;
                    }
                    else
                        inCombat = false;
                }
                else
                {
                    inCombat = false;
                }
            }
            for (int i = 0; i < Game1.testBoss.mobs.Count; i++)
            {
                if (Game1.testBoss.mobs[i].health > 0 && Game1.testBoss.mobs[i].IsOnMap() && Game1.testBoss.mobs[i].vulnerable)
                {
                    if (Game1.testBoss.mobs[i].combatRectangle.Intersects(position))
                    {
                        inCombat = true;
                    }
                    else
                        inCombat = false;
                }
                else
                {
                    inCombat = false;
                }
            }
            #endregion
        }

        public void DeathEffect(Game1 game)
        {
            if (game.screenColor.A < 255)
            {
                game.screenColor.A += 5;
            }
        }

        #region Particle Effects
        public void Hit()
        {
            emitter.Add(position.X + 24, position.Y + 24, 6, 6, 10, -4, 4, -4, 4, new Color(255, 10, 10), 0.35f, 1, 1, false, false, true);
            isHit = true;
        }

        public void Bleed()
        {
            emitter.Add(position.X + 24, position.Y + 24, 6, 6, 1, 0, 0, -1, 3, new Color(255, 10, 10), 0.07f, 1, 1, false, false, true);
        }

        public void Regen(int amount)
        {
            emitter.Add(position.X + 24, position.Y + 24, 6, 6, amount * 2, -4, 4, -4, 4, new Color(10, 240, 10), 0.7f, 1, 1, false, false, false);
        }

        public void Walk()
        {
            emitter.Add(position.X + 22, position.Y + position.Height - 5, 6, 6, 1, -2, 2, -3, -1, new Color(rand.Next(50, 65), rand.Next(35, 50), rand.Next(35, 50), rand.Next(100, 250)), 0.0f, 1, 1, false, false, true);
        }

        public void Splash()
        {
            emitter.Add(position.X + 22, position.Y + position.Height - 5, rand.Next(5, 8), rand.Next(5, 8), 6, -2, 2, -3, -1, new Color(50, 50, 255), 0.05f, 1, 1, false, false, true);
            emitter.Add(position.X + 22, position.Y + position.Height - 5, rand.Next(5, 8), rand.Next(5, 8), 6, -2, 2, -3, -1, new Color(200, 200, 200), 0.05f, 1, 1, false, false, true);
        }
        #endregion

        public bool IsCollision(H_Map.TileMap tiles, Rectangle location)
        {
            Point tileIndex = tiles.GetTileIndexFromVector(new Vector2(location.X, location.Y));
            return (tiles.interactiveLayer[tileIndex.X, tileIndex.Y].isPassable == false);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle shadowPos = new Rectangle(position.X + 8, position.Bottom - shadowBlob.Height / 2, shadowBlob.Width, shadowBlob.Height);
            spriteBatch.Draw(shadowBlob, shadowPos, Color.White);
            spriteBatch.Draw(debugTexture, attackRectangle, new Color(100, 100, 100, 100));
            //spriteBatch.Draw(debugTexture, interactRect, new Color(100, 100, 100, 100));
            spriteBatch.Draw(spriteWalkSheet, position, animation.GetFrame(), Color.White);
        }

        public void DrawDeath(SpriteBatch spriteBatch, Game1 game)
        {
            spriteBatch.Draw(game.screenTexture, game.screen, game.screenColor);
        }

        public void DrawHealthBar(SpriteBatch spriteBatch, Game1 game)
        {
            if (healthTexture != null)
            {
                spriteBatch.Draw(healthTexture, new Rectangle(healthPos.X, healthPos.Y, healthPos.Width - 2, healthPos.Height), Color.White);
                spriteBatch.Draw(healthBar, new Rectangle(healthPos.X - 7, healthPos.Y - 15, 64, 30), Color.White);
                for (int i = 0; i < damageEffectList.Count; i++)
                {
                    damageEffectList[i].Draw(spriteBatch, game);
                }
            }
        }

        public void Push(Vector2 direction)
        {

        }
    }
}
