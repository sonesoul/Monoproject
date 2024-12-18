﻿using Engine;
using Engine.Drawing;
using Engine.Modules;
using Engine.Types;
using GlobalTypes;
using GlobalTypes.Events;
using GlobalTypes.InputManagement;
using InGame.GameObjects;
using InGame.Generators;
using SharpDX.MediaFoundation;

namespace InGame
{
    public class GameMain 
    {
        private Player player;
        public GameMain()
        {
            FrameEvents.Update.Add(Update, UpdateOrders.GameMain);
            CreateWalls();

            player = new();
            Level.AddObject(player);

            Level.Load();
        }

        private void Update()
        {
        }

        private static void CreateWalls()
        {
            int width = MainContext.WindowWidth;
            int height = MainContext.WindowHeight;
            int offset = 9;

            //top
            _ = new StringObject("", UI.Silk, true,
                new Collider()
                {
                    Shape = Polygon.Rectangle(width, 20),
                },
                new Rigidbody()
                {
                    BodyType = BodyType.Static,
                }).Position = new(width / 2, -offset);

            //bottom
            _ = new StringObject("", UI.Silk, true,
                new Collider()
                {
                    Shape = Polygon.Rectangle(width + 1, 20),
                }, 
                new Rigidbody()
                {
                    BodyType = BodyType.Static,
                }).Position = new(width / 2 - 1, height + offset + 1f);

            //right
            _ = new StringObject("", UI.Silk, true,
                new Collider()
                {
                    Shape = Polygon.Rectangle(20, height + 1),
                }, 
                new Rigidbody()
                {
                    BodyType = BodyType.Static,
                }).Position = new(width + offset + 1, height / 2);

            //left
            _ = new StringObject("", UI.Silk, true, 
                new Collider()
                {
                    Shape = Polygon.Rectangle(20, height),
                }, 
                new Rigidbody()
                {
                    BodyType = BodyType.Static,
                }).Position = new(-offset, height / 2);
        }
    }
}