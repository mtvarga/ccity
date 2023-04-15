﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class MainModel
    {
        #region Fields

        private FieldManager _fieldManager;
        private CitizenManager _citizenManager;
        private GlobalManager _globalManager;

        #endregion

        #region Properties

        public string CityName { get; private set; }
        public string MayorName { get; private set; }
        public Field[][] Fields { get; private set; }
        public List<Citizen> Citizens { get; }
        public int GlobalSatisfactionScore { get; }
        public int Budget { get; }
        public Taxes Taxes { get; }
        public int Date { get; }
        public Speed Speed { get; }
        public int Satisfaction { get; }
        public int Population { get; }


        #endregion

        #region Constructors

        public MainModel()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public methods

        public void Place(int x, int y, PlaceableType type)
        {
            throw new NotImplementedException();
        }

        public void Demolish(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void Upgrade(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void SendFiretruck(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void ChangeTax(TaxType type, int n)
        {
            throw new NotImplementedException();
        }

        public void ChangeSpeed(int n)
        {
            throw new NotImplementedException();
        }

        public void TimerTick()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private methods

        private void ChangeDate()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Events

        public EventHandler<EventArgs> GameTicked;
        public EventHandler<FieldEventArgs> FieldsUpdated;
        public EventHandler<EventArgs> PopulationChanged;
        public EventHandler<EventArgs> BudgetChanged;
        public EventHandler<EventArgs> SatisfactionChanged;
        public EventHandler<EventArgs> TaxChanged;
        public EventHandler<EventArgs> GameOver;

        #endregion
    }
}
