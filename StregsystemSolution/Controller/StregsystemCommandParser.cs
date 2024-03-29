﻿using System;
using System.Collections.Generic;
using Core;
using Core.Exceptions;
using Core.Transactions;
using UserInterface;

namespace Controller
{
    public class StregsystemCommandParser
    {
        private IStregsystemUI _stregsystemUI;
        private IStregsystem _stregsystem;
        private Dictionary<string, Action<string[]>> _adminCommands;

        public StregsystemCommandParser(IStregsystemUI stregsystemUI, IStregsystem stregsystem)
        {
            _stregsystemUI = stregsystemUI;
            _stregsystem = stregsystem;

            AdminCommands adminCommands = new AdminCommands(stregsystemUI, stregsystem);
            _adminCommands = new Dictionary<string, Action<string[]>>()
            {
                {":q", adminCommands.Quit},
                {":quit", adminCommands.Quit},
                {":activate", adminCommands.ActivateProduct},
                {":deactivate", adminCommands.DeactivateProduct},
                {":crediton", adminCommands.CreditOnProduct},
                {":creditoff", adminCommands.CreditOffProduct},
                {":addcredits", adminCommands.AddCredits}
            };
        }

        public void AdminCommandParse(string command)
        {
            string[] splittedCommand = command.Split(" ");
            foreach (KeyValuePair<string, Action<string[]>> adminCommand in _adminCommands)
            {
                if (splittedCommand[0] == adminCommand.Key)
                {
                    adminCommand.Value(splittedCommand);
                    return;
                }
            }
            _stregsystemUI.DisplayAdminCommandNotFoundMessage(command);
        }

        public void CommandParse(string command)
        {
            string[] splittedCommand = command.Split(" ");
            
            User user;

            try
            {
                user = _stregsystem.GetUserByUsername(splittedCommand[0]);
            }
            catch (UserDoesntExistException e)
            {
                _stregsystemUI.DisplayUserNotFound(e.Username);
                return;
            }
            
            switch (splittedCommand.Length)
            {
                // Showing user info
                case 1: _stregsystemUI.DisplayUserInfo(user);
                    break;
                
                // Buying 1 product
                case 2 : UserBuyProductCommand(user, 1, Convert.ToInt32(splittedCommand[1]));
                    break;
                
                // Buying >1 products
                case 3 : UserBuyProductCommand(user, Convert.ToInt32(splittedCommand[1]), Convert.ToInt32(splittedCommand[2]));
                    break;
                
                default: _stregsystemUI.DisplayTooManyArgumentsError(command);
                    break;
            }
        }

        private void UserBuyProductCommand(User user, int amount, int productID)
        {
            Product product;
            try
            {
                product = _stregsystem.GetProductByID(productID);
            }
            catch (ProductDoesntExistException e)
            {
                _stregsystemUI.DisplayProductNotFound(e.ProductID);
                return;
            }

            BuyTransaction buyTransaction = _stregsystem.BuyProduct(user, product, amount);
            try
            {
                buyTransaction.Execute();
            }
            catch (InsufficientCreditsException e)
            {
                _stregsystemUI.DisplayInsufficientCash(e.User, e.Product);
                return;
            }
            
            _stregsystem.AddTransaction(buyTransaction);
            _stregsystemUI.DisplayUserBuysProduct(amount, buyTransaction);
        }
        
    }
}