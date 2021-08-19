﻿using System;
using TheGame.Common.Models;

namespace TheGame.DataService
{
    public interface IPlayerRepository : IGenericRepository<Player> , IDisposable
    {

    }
}
