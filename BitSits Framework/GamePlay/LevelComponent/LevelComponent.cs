﻿/*
 * Copyright (c) 2011 BitSits Games
 *  
 * Shubhajit Saha    http://bitsits.blogspot.com/
 * Maya Agarwal      http://bitsitsgames.blogspot.com/
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Box2D.XNA;
using GameDataLibrary;

namespace BitSits_Framework
{
    class LevelComponent
    {
        protected GameContent gameContent;
        protected World world;

        public List<Atom> atoms = new List<Atom>();
        public List<Equipment> equipments = new List<Equipment>();

        protected readonly Vector2 entryPoint;

        public bool IsLevelUp;
        public bool ReloadLevel;

        protected readonly int MaxAtoms;

        public readonly BonusType bonusType;

        LevelData levelData;

        protected Equipment thermometer, pHscale;

        public LevelComponent(GameContent gameContent, World world)
        {
            this.gameContent = gameContent;
            this.world = world;

            levelData = gameContent.content.Load<LevelData>("Levels/level" + gameContent.levelIndex);

            MaxAtoms = levelData.MaxAtoms;

            int totalProbability = 0;
            for (int i = 0; i < levelData.AtomProbability.Length; i++) totalProbability += levelData.AtomProbability[i];

            if (totalProbability != 100) throw new Exception("must be 100");

            entryPoint = levelData.Entry;

            bonusType = levelData.BonusType;

            BodyDef bd = new BodyDef();
            Body ground = world.CreateBody(bd);

            PolygonShape ps = new PolygonShape();

            List<Vector2> v = levelData.ContinuousBoundry;
            for (int i = 0; i < v.Count - 1; i++)
            {
                if (v[i + 1].X == -1 && v[i + 1].Y == -1) { i++; continue; }

                ps.SetAsEdge(v[i] / gameContent.scale, v[i + 1] / gameContent.scale);
                ground.CreateFixture(ps, 0);
            }

            for (int i = 0; i < levelData.EquipmentDetails.Count; i++)
            {
                Equipment eq = new Equipment(levelData.EquipmentDetails[i].EquipmentName, gameContent, world);
                eq.body.Position = levelData.EquipmentDetails[i].Position / gameContent.scale;
                eq.body.Rotation = levelData.EquipmentDetails[i].RotationInDeg / 180 * (float)MathHelper.Pi;
                eq.isClamped = levelData.EquipmentDetails[i].IsClamped;
                eq.body.SetType(BodyType.Static);
                equipments.Add(eq);

                if (eq.equipName == EquipmentName.thermometer) thermometer = eq;

                if (eq.equipName == EquipmentName.pHscale) pHscale = eq;
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            for (int i = (MaxAtoms - atoms.Count) - 1; i >= 0; i--)
            {
                int total = 0, a;
                a = gameContent.random.Next(100) + 1;

                for (int j = 0; j < levelData.AtomProbability.Length; j++)
                {
                    if (total < a && a <= total + levelData.AtomProbability[j])
                    {
                        atoms.Add(new Atom((Symbol)(j), entryPoint, gameContent, world)); break;
                    }
                    total += levelData.AtomProbability[j];
                }
            }
        }

        public virtual bool UpdateNewFormula(Formula formula) { return false; }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(gameContent.menuBackground, 
                new Rectangle(0, 0, (int)gameContent.viewportSize.X, (int)gameContent.viewportSize.Y), Color.White);
            spriteBatch.Draw(gameContent.levelBackground[gameContent.levelIndex], Vector2.Zero, Color.White);
        }
    }
}
