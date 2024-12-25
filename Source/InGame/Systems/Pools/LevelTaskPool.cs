using GlobalTypes;
using InGame.Interfaces;
using InGame.LevelTasks;
using InGame.Pools;
using System;
using System.Collections.Generic;

namespace InGame.Pools
{
    public static class LevelTaskPool
    {
        private static List<Func<ILevelTask>> taskCreators = new()
        {
            () => new PointTouchTask(),
            () => new ZoneFollowTask(),
        };
        private static IndexPool indexes = new(taskCreators);

        public static ILevelTask GetRandom() => taskCreators[indexes.Pop()]();
    }
}