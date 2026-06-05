
        #region при получении штрихкода с камеры/сканера

        /// <summary>
        /// при получении штрихкода с камеры/сканера
        /// </summary>
        /// <param name="barcode">штрихкод товара</param>
        /// <param name="type">тип штрихкода</param>
        /// <param name="HandEnter">был ли зафиксирован ручной ввод кода товара или штрихкода</param>
        public /*async*/ void ScanerInit_BarcodeDataEvent(string barcode, string type)
        //public /*async*/ void ScanerInit_BarcodeDataEvent(string barcode, string type)
        {
            try
            {
                //если статус сканера активен
                if (cs.ScanerInit.StatusScanner)
                {
                    //добавил блок 02.02.2026
                    cs.Logs.TimeMeasureLoggerAsync.Instance.Start();
                    //КОНЕЦ добавил блок 02.02.2026
                    //cs.ScanerInit.StatusScanner = false;
                    if (string.IsNullOrWhiteSpace(barcode))
                    {
                        //this?.SafeRunOnUiThread(() =>
                        //{
                            cs.Toasts.aToast.ShowToast(this, /*"Штрихкод не распознан сканером"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message38), ToastLength.Short, true);
                        //});
                        return;
                    }

                    //добавил строку 20.11.2025_#1
                    barcode = cs.AES.RijndaelCryptoHelper.DecryptBarcode(barcode);

                    #region если коментарий к текущему документу

                    if (cs.DocDetails.ComentDialog.ActiveDialog)
                    {
                        cs.DialogFragments.BaseDialogFragment.dialogFragment35.newNameBarcodeOrCodeProduct.Text = barcode;
                        return;
                    }
                    #endregion

                    #region если штрихкод пользователя

                    if (!string.IsNullOrWhiteSpace(barcode))
                    {
                        if (barcode.IndexOf("L:") > -1 && barcode.IndexOf("P:") > 2)
                        {
                            //добавил блок #_5 21.02.2024
                            if (cs.BaseClass.OpenHandCard)
                            {
                                return;
                            }
                            if (productDialog != null)
                            {
                                if (productDialog.isOpened)
                                {
                                    return;
                                }
                            }
                            //end добавил блок #_5 21.02.2024
                            try
                            {
                                int indexL = (barcode.IndexOf("L:")) + 2;
                                int indexP = (barcode.LastIndexOf("P:")) + 2;
                                string login = barcode.Substring(indexL, indexP - 4);
                                string password = barcode.Substring(indexP, barcode.Length - indexP);
                                if (indexL > -1 && indexP > -1)
                                {
                                    //this.RunOnUiThread(() =>
                                    //{
                                    //    cs.Toasts.aToast.ShowToast(this, "Штрихкод " + barcode + " не найден", ToastLength.Long, true);
                                    //});
                                    return;
                                }
                            }
                            catch { }
                        }
                    }

                    #endregion

                    #region если открыто окно создания нового документа
                    //добавил блок 15.11.2024
                    try
                    {
                        string maskaBarcodeDcu2 = cs.BaseClass.preferences.GetString("PrefixBarcodeDocu", "$Doc");
                        if (maskaBarcodeDcu2.Length > 0 && barcode.Length > maskaBarcodeDcu2.Length)
                        {
                            if (barcode.Substring(0, maskaBarcodeDcu2.Length) == maskaBarcodeDcu2)
                            {
                                if (newNameDocuDialog != null)
                                {
                                    if (newNameDocuDialog.isOpened)
                                    {
                                        newNameDocuDialog.BarcodeDocu.Text = barcode.Remove(0, maskaBarcodeDcu2.Length);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ex = ex;
                    }
                    //КОНЕЦ добавил блок 15.11.2024
                    #endregion

                    #region если штрихкод документа

                    string maskaBarcodeDcu = cs.BaseClass.preferences.GetString("PrefixBarcodeDocu", "$Doc");
                    if (maskaBarcodeDcu.Length > 0)
                    {
                        if (barcode.Length > maskaBarcodeDcu.Length)
                        {
                            if (barcode.Substring(0, maskaBarcodeDcu.Length) == maskaBarcodeDcu)
                            {
                                //добавил блок #_4 21.02.2024
                                if (cs.BaseClass.OpenHandCard)
                                {
                                    return;
                                }
                                if (productDialog != null)
                                {
                                    if (productDialog.isOpened)
                                    {
                                        return;
                                    }
                                }
                                //end добавил блок #_4 21.02.2024

                                try
                                {
                                    using (SmartStoreData.SourceDataBase dataBase = new SmartStoreData.SourceDataBase())
                                    {
                                        dataBase.SourceDataBaseMethod(cs.BaseClass.pathDocs);
                                        if (dataBase.ContextBDExsists)
                                        {
                                            dataBase.dataContext.generateLogExceptionProcess = this;
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            List<SmartStoreData.DocHead> vremDoc = dataBase.dataContext.DocHead
                                                .Where(c => c.BarcodeDocu ==
                                                            barcode.Remove(0,
                                                                maskaBarcodeDcu
                                                                    .Length)) /*.Where(c=>c.UserF==cs.BaseClass.currentIdUser || c.UserF==-1)*/
                                                .ToList_Ext();
                                            if (vremDoc.Count == 1)
                                            {
                                                if (vremDoc[0].UserF != -1)
                                                {
                                                    if (vremDoc[0].UserF != cs.BaseClass.currentIdUser)
                                                    {
                                                        return;
                                                    }
                                                }

                                                //добавил блок 15.11.2024
                                                bool prodOper = false;
                                                if (cs.BaseClass.currentDocHead == null)
                                                {
                                                    prodOper = true;
                                                }
                                                else
                                                {
                                                    if (cs.BaseClass.currentDocHead.BarcodeDocu != vremDoc[0].BarcodeDocu)
                                                    {
                                                        prodOper = true;
                                                    }
                                                }
                                                //КОНЕЦ добавил блок 15.11.2024
                                                //15.11.2024 заменил cs.BaseClass.currentDocHead.BarcodeDocu != vremDoc[0].BarcodeDocu на prodOper
                                                if (prodOper)
                                                {
                                                    MainActivity.BarcodeDocuOpen = vremDoc[0].BarcodeDocu;
                                                    //cs.BaseClass.currentDocHead = vremDoc[0];
                                                    this.FinishAndRemoveTask();
                                                    this.OnBackPressed();
                                                    //Intent intent = new Intent(this, typeof(MainActivity));
                                                    //intent.AddFlags(ActivityFlags.ClearTop);
                                                    //StartActivity(intent);
                                                    return;
                                                }
                                                else
                                                {

                                                    cs.Toasts.aToast.ShowToast(this, /*"Документ уже открыт"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message40),
                                                        ToastLength.Long, true, 1);
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                if (vremDoc.Count > 1)
                                                {
                                                    //реализовать вывод списка найденных документов с указанным штрихкодом документа
                                                }
                                                else
                                                {
                                                    if (vremDoc.Count == 0)
                                                    {
                                                        this.RunOnUiThread(() =>
                                                        {
                                                            cs.Toasts.aToast.ShowToast(this, /*"Документ "*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle) + " " +/*"с штрихкодом "*/Android.App.Application.Context.GetText(Resource.String.WithBarcode) + " " + barcode.Remove(0, maskaBarcodeDcu.Length) + " " +/*"не найден"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message8), ToastLength.Long, true);
                                                        });
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            cs.AlerrtDialogs.MessageNotAvailableContextDataBase.ShowDialog(this, "$34");
                                            return;
                                        }
                                    }
                                }
                                finally
                                {
                                    cs.BaseClass.GC();
                                }
                            }

                        }
                    }

                    #endregion

                    #region если документ не привязан к текущему пользователю или был удален

                    using (SmartStoreData.SourceDataBase dataBase = new SmartStoreData.SourceDataBase())
                    {
                        dataBase.SourceDataBaseMethod(cs.BaseClass.pathDocs);
                        if (dataBase.ContextBDExsists)
                        {
                            dataBase.dataContext.generateLogExceptionProcess = this;
                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                            var docuser = dataBase.dataContext.DocHead
                                .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).ToList_Ext();
                            if (docuser.Count == 0)
                            {
                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                    .SetMessage(/*"Отсутствует текущий документ! Возможно документ был удален"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message38))
                                    .SetCancelable(false)
                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                    .SetPositiveButton(/*"Закрыть"*/Android.App.Application.Context.GetText(Resource.String.Close_button), (senderAlert, args) => { this.FinishAndRemoveTask(); })
                                    .Show();
                                return;
                            }
                            else
                            {
                                if (cs.BaseClass.currentloginUser != "sa")
                                {
                                    if (docuser[0].UserF != -1)
                                    {
                                        if (docuser[0].UserF != cs.BaseClass.currentIdUser)
                                        {
                                            new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                .SetMessage(/*"Текущий документ был подхвачен другим пользователем"*/Android.App.Application.Context.GetText(Resource.String.NotCurrentUserDocu))
                                                .SetCancelable(false)
                                                .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                .SetPositiveButton(/*"Закрыть"*/Android.App.Application.Context.GetText(Resource.String.Close_button),
                                                    (senderAlert, args) => { this.FinishAndRemoveTask(); })
                                                .Show();
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            this?.RunOnUiThread(() =>
                            {
                                cs.AlerrtDialogs.MessageNotAvailableContextDataBase.ShowDialog(this, "$31");
                            });
                            return;
                        }
                    }

                    #endregion

                    //добавил блок 25.12.2025
                    #region проверяем, является ли штрихкод, штрихкодом типа Ящик или Паллета
                    if (cs.BaseClass.currentDocHead != null)
                    {
                        if (cs.BaseClass.currentDocHead?.DocType == 10)
                        {
                            if (Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.IsWaveAssemblyDocument())
                            {
                                //закоментировал строку 14.01.2026
                                //Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.DetermineTypeBarcode(barcode);
                                //14.01.2026 заменил /*currentTypeBarcode*/ на IsValidTypeBarcode(barcode)
                                switch (Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance./*currentTypeBarcode*/IsValidTypeBarcode(barcode))
                                {
                                    case Layers.Documents.WaveAssembly.TypeBarcode.Box:
                                        //выполнить необходимые действия (поже реализую)
                                        break;
                                    case Layers.Documents.WaveAssembly.TypeBarcode.Pallet:
                                        //добавил блок 14.01.2025
                                        cs.BaseClass.GetCurrentActivity()?.RunOnUiThread(() =>
                                        {
                                            if (progressDialog != null)
                                            {
                                                try
                                                {
                                                    progressDialog.Dismiss();
                                                }
                                                catch
                                                {
                                                }

                                                progressDialog = null;
                                            }

                                            progressDialog = new Android.App.ProgressDialog(this, Android.App.ProgressDialog.ThemeHoloDark);
                                            progressDialog.SetCancelable(false);
                                            progressDialog.SetCanceledOnTouchOutside(false);
                                            progressDialog.SetMessage(/*"Обработка данных"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message30) + "...");
                                            progressDialog.Show();
                                        });

                                        var resTask = Task.Run(() =>
                                        {
                                            bool res = Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.CanAddPalletToDocument(barcode, true);
                                            cs.BaseClass.GetCurrentActivity()?.RunOnUiThread(() =>
                                            {
                                                progressDialog.Dismiss();
                                            });
                                            if (res)
                                            {
                                                cs.BaseClass.GetCurrentActivity()?.RunOnUiThread(() =>
                                                {
                                                    if (PalletNameLinearLayout.Visibility != ViewStates.Visible)
                                                    {
                                                        PalletNameLinearLayout.Visibility = ViewStates.Visible;
                                                    }
                                                    palletNameTextView.Text = Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.GetPalletBarcode();
                                                });
                                                return;
                                            }
                                            else
                                            {
                                                Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.ClearPalletContent();
                                                cs.BaseClass.GetCurrentActivity()?.RunOnUiThread(() =>
                                                {
                                                    if (PalletNameLinearLayout.Visibility != ViewStates.Visible)
                                                    {
                                                        PalletNameLinearLayout.Visibility = ViewStates.Visible;
                                                    }
                                                    palletNameTextView.Text = null;
                                                });
                                                return;
                                            }
                                        });
                                        return;
                                        //КОНЕЦ добавил блок 14.01.2025
                                        /*break*/
                                        ;
                                }
                            }

                            //добавил блок 14.01.2025
                            if (!Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.palletContent.IsPalletSelected)
                            {
                                cs.BaseClass.GetCurrentActivity()?.RunOnUiThread(() =>
                                {
                                    cs.Toasts.aToast.ShowToast(cs.BaseClass.GetCurrentActivity(), /*"Необходимо отсканировать штрихкод типа "Паллета""*/cs.BaseClass.GetCurrentActivity()?.GetText(Resource.String.WaveAssembly_Message1), ToastLength.Long, true);
                                });
                                return;
                            }
                            //КОНЕЦ добавил блок 14.01.2025
                        }
                    }
                    #endregion
                    //КОНЕЦ добавил блок 25.12.2025

                    #region если штрихкод ячейки
                    isBarcodeCellFind = false;

                    //добавил 29.01.2024
                    //содержит штрихкод ячейки и используется для вставки в компонент поиска
                    string barcodeFindComponent = null;

                    //добавил 03.02.2025
                    //ножно ли вставлять штрихкод в компонент поиска (используется для вставки в компонент поиска)
                    bool barcodeFindComponent_Insert = true;

                    if (cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                    {
                        string maskaCell = cs.BaseClass.preferences.GetString("MaskCellEditText", "#");
                        if (maskaCell.Length > 0)
                        {
                            if (barcode.Length > maskaCell.Length)
                            {
                                if (barcode.Substring(0, maskaCell.Length) == maskaCell)
                                {
                                    //добавил блок #_3 21.02.2024
                                    if (cs.BaseClass.OpenHandCard)
                                    {
                                        return;
                                    }

                                    if (productDialog != null)
                                    {
                                        if (productDialog.isOpened)
                                        {
                                            return;
                                        }
                                    }

                                    //end добавил блок #_3 21.02.2024
                                    #region если фокус на компоненте поиска

                                    //условие добавлено 22.01.2024
                                    if (searchView.HasFocus)
                                    {
                                        barcodeFindComponent = barcode.Remove(0, maskaCell.Length);
                                        isBarcodeCellFind = true;
                                        //barcode = barcode.Remove(0, maskaCell.Length);

                                        try
                                        {
                                            using (SmartStoreData.SourceDataBase dataBase = new SmartStoreData.SourceDataBase())
                                            {
                                                dataBase.SourceDataBaseMethod(cs.BaseClass.pathDocs);
                                                if (dataBase.ContextBDExsists)
                                                {
                                                    dataBase.dataContext.generateLogExceptionProcess = this;
                                                    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                    List<SmartStoreData.Cell> vremcells2 = dataBase.dataContext.Cell
                                                        .Where(c => c.BarcodeCell == barcodeFindComponent).ToList_Ext();

                                                    if (vremcells2.Count == 1)
                                                    {
                                                        ////добавил 10.02.2025_#2
                                                        //isBarcodeCellFind = true;
                                                        barcodeFindComponent = vremcells2[0].Name;
                                                        //добавил блок 03.02.2025
                                                        if (cs.BaseClass.currentDocHead != null && vremcells2 != null)
                                                        {
                                                            if (vremcells2.Count > 0)
                                                            {
                                                                if (!vremcells2[0].isPackingArea && cs.BaseClass.currentDocHead.DocType == 4
                                                                        && cs.BaseClass.currentDocHead.SubDocType == "selling"
                                                                        && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_SellingCheckBox", false)
                                                                        && cs.BaseClass.currentDocHead.StateMovedFrom
                                                                        && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false)
                                                                        && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                                {
                                                                    this.RunOnUiThread(() =>
                                                                    {
                                                                        cs.Toasts.aToast.ShowToast(this,
                                                                        /*"Можно добавлять только ячейки зоны упаковки"*/Android.App.Application.Context.GetText(Resource.String.ForTypesDocuments_Moving_isPackingAreaCells_Message),
                                                                        ToastLength.Short, true, 1);
                                                                    });
                                                                    //cs.BaseClass.currentIdCell = null;
                                                                    //cs.BaseClass.currentNameCell = null;
                                                                    //cellNameTextView.Text = null;
                                                                    barcodeFindComponent_Insert = false;
                                                                }
                                                                //добавил блок 04.03.2025_#1
                                                                else
                                                                {
                                                                    if (vremcells2[0].isPackingArea && vremcells2[0].isReservation && cs.BaseClass.currentDocHead.DocType == 4
                                                                        && cs.BaseClass.currentDocHead.SubDocType == "selling"
                                                                        && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_SellingCheckBox", false)
                                                                        && cs.BaseClass.currentDocHead.StateMovedFrom
                                                                        && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false)
                                                                        && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                                    {
                                                                        //добавил блок 11.03.2025
                                                                        bool breakOperation = cs.BaseClass.BreakTekCellReservedControl(vremcells2);
                                                                        if (breakOperation)
                                                                        {
                                                                            //КОНЕЦ добавил блок 11.03.2025

                                                                            this.RunOnUiThread(() =>
                                                                            {
                                                                                cs.Toasts.aToast.ShowToast(this,
                                                                                /*"Текущая ячейка забронирована со стороны учетной системы"*/Android.App.Application.Context.GetText(Resource.String.ForTypesDocuments_Moving_isReservationAreaCells_Message),
                                                                                ToastLength.Short, true, 1);
                                                                            });
                                                                            //cs.BaseClass.currentIdCell = null;
                                                                            //cs.BaseClass.currentNameCell = null;
                                                                            //cellNameTextView.Text = null;
                                                                            barcodeFindComponent_Insert = false;
                                                                            //добавил блок 11.03.2025
                                                                        }
                                                                        //КОНЕЦ добавил блок 11.03.2025
                                                                    }
                                                                }
                                                                //КОНЕЦ добавил блок 04.03.2025_#1
                                                            }
                                                        }
                                                        //КОНЕЦ добавил блок 03.02.2025
                                                    }
                                                }
                                                else
                                                {
                                                    this?.RunOnUiThread(() =>
                                                    {
                                                        cs.AlerrtDialogs.MessageNotAvailableContextDataBase.ShowDialog(this, "$35");
                                                    });
                                                    return;
                                                }
                                            }
                                        }
                                        catch (System.Exception ex)
                                        {
                                            using (GenerateLogException generateLog = new GenerateLogException())
                                            {
                                                generateLog.InfoException(ex, "ListDocDetailsActivity.ScanerInit_BarcodeDataEvent_#f1", cs.BaseClass.FreeMemoryMessage());
                                            }
                                            ScanerInit_BarcodeDataEvent(barcode, type);
                                            return;
                                        }
                                        finally
                                        {
                                            cs.BaseClass.GC();
                                        }

                                    }
                                    #endregion
                                    #region иначе, если фокус не на компоненте поиска
                                    else
                                    {
                                        SmartStoreData.Cell cells = null;
                                        try
                                        {
                                            using (SmartStoreData.SourceDataBase dataBase = new SmartStoreData.SourceDataBase())
                                            {
                                                dataBase.SourceDataBaseMethod(cs.BaseClass.pathDocs);
                                                if (dataBase.ContextBDExsists)
                                                {
                                                    dataBase.dataContext.generateLogExceptionProcess = this;
                                                    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                    List<SmartStoreData.Cell> vremcells = dataBase.dataContext.Cell/*.AsNoTracking()*/
                                                        .Where(c => c.BarcodeCell == barcode.Remove(0, maskaCell.Length)).ToList_Ext();
                                                    //List<SmartStoreData.Cell> vremcells = dataBase.dataContext.Cell
                                                    //    .Where(c => c.BarcodeCell == barcode).ToList_Ext();
                                                    if (vremcells.Count == 0)
                                                    {
                                                        //добавил условие 10.02.2025_#2
                                                        if (cs.BaseClass.preferences.GetBoolean("SaveNewCellCheckBox", true))
                                                        {
                                                            SmartStoreData.Cell cell = new SmartStoreData.Cell();
                                                            cell.BarcodeCell = barcode.Remove(0, maskaCell.Length);
                                                            cell.Name = barcode.Remove(0, maskaCell.Length);
                                                            //cell.BarcodeCell = barcode;
                                                            //cell.Name = barcode;
                                                            cell.Updated = true;
                                                            try
                                                            {
                                                                LinqExtensions.WriteErrorToFile = false;
                                                                //29.08.2025 заменил .Max на .Max_Ext
                                                                cell.id = dataBase.dataContext.Cell/*.AsNoTracking()*/.Max_Ext(c => c.id) + 1;
                                                                LinqExtensions.WriteErrorToFile = true;
                                                            }
                                                            catch
                                                            {
                                                                cell.id = 1;
                                                            }

                                                            cell.CellF = cell.id;
                                                            dataBase.dataContext.Cell.Add(cell);
                                                            if (dataBase.dataContext.SaveChanges() != 1)
                                                            {
                                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                    .SetMessage(/*"Не удалось добавить ячейку в справочник"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_SelectedCellDialog_Message9))
                                                                    .SetCancelable(false)
                                                                    .SetTitle(/*"База данных"*/Android.App.Application.Context.GetText(Resource.String.DataBase))
                                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK), (senderAlert, args) => { })
                                                                    .Show();
                                                                return;
                                                            }
                                                            else
                                                            {
                                                                //добавил 10.02.2025_#2
                                                                isBarcodeCellFind = true;
                                                                cells = cell;
                                                            }
                                                        }
                                                        //добавил блок 10.02.2025_#2
                                                        else
                                                        {
                                                            this.RunOnUiThread(() =>
                                                            {
                                                                //new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                //                          .SetMessage(/*"Укажите существующий штрихкод ячейки"*/this.GetText(Resource.String.cs_DocDetails_SelectedCellDialog_Message12))
                                                                //                          .SetCancelable(false)
                                                                //                          .SetTitle(/*"Ошибка ввода/выбора"*/this.GetText(Resource.String.cs_DocDetails_SelectedCellDialog_Message3))
                                                                //                          .SetPositiveButton(/*"OK"*/this.GetText(Resource.String.OK), (senderAlert, args) => { })
                                                                //                          .Show();

                                                                cs.Toasts.aToast.ShowToast(this,
                                                                /*"Укажите существующий штрихкод ячейки"*/this.GetText(Resource.String.cs_DocDetails_SelectedCellDialog_Message12),
                                                                ToastLength.Short, true, 1);
                                                                cs.BaseClass.currentIdCell = null;
                                                                cs.BaseClass.currentNameCell = null;

                                                                cellNameTextView.Text = null;


                                                            });
                                                            return;
                                                        }
                                                        //КОНЕЦ добавил блок 10.02.2025_#2
                                                    }
                                                    else
                                                    {
                                                        //01.09.2025 заменил имя метода First на First_Ext
                                                        cells = vremcells.First_Ext();
                                                        //добавил 10.02.2025_#2
                                                        isBarcodeCellFind = true;
                                                    }
                                                }
                                                else
                                                {

                                                    cs.AlerrtDialogs.MessageNotAvailableContextDataBase.ShowDialog(this, "$32");
                                                    return;
                                                }
                                            }
                                        }
                                        catch (System.Exception ex)
                                        {
                                            using (GenerateLogException generateLog = new GenerateLogException())
                                            {
                                                generateLog.InfoException(ex, "ListDocDetailsActivity.ScanerInit_BarcodeDataEvent_#f2", cs.BaseClass.FreeMemoryMessage());
                                            }
                                            ScanerInit_BarcodeDataEvent(barcode, type);
                                            return;
                                        }
                                        finally
                                        {
                                            cs.BaseClass.GC();
                                        }

                                        //добавил блок 03.02.2025
                                        if (cs.BaseClass.currentDocHead != null)
                                        {
                                            if (!cells.isPackingArea && cs.BaseClass.currentDocHead.DocType == 4
                                                    && cs.BaseClass.currentDocHead.SubDocType == "selling"
                                                    && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_SellingCheckBox", false)
                                                    && cs.BaseClass.currentDocHead.StateMovedFrom
                                                    && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false)
                                                    && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                            {
                                                this.SafeRunOnUiThread(() =>
                                                {
                                                    cs.Toasts.aToast.ShowToast(this,
                                                    /*"Можно добавлять только ячейки зоны упаковки"*/Android.App.Application.Context.GetText(Resource.String.ForTypesDocuments_Moving_isPackingAreaCells_Message),
                                                    ToastLength.Short, true, 1);
                                                });
                                                cs.BaseClass.currentIdCell = null;
                                                cs.BaseClass.currentNameCell = null;
                                                this.SafeRunOnUiThread(() =>
                                                {
                                                    cellNameTextView.Text = null;
                                                });
                                                return;
                                            }
                                            //добавил блок 04.03.2025_#1
                                            else
                                            {
                                                if (cells.isPackingArea && cells.isReservation && cs.BaseClass.currentDocHead.DocType == 4
                                                    && cs.BaseClass.currentDocHead.SubDocType == "selling"
                                                    && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_SellingCheckBox", false)
                                                    && cs.BaseClass.currentDocHead.StateMovedFrom
                                                    && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false)
                                                    && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                {
                                                    //добавил блок 11.03.2025
                                                    bool breakOperation = cs.BaseClass.BreakTekCellReservedControl(cells);
                                                    if (breakOperation)
                                                    {
                                                        //КОНЕЦ добавил блок 11.03.2025
                                                        this.SafeRunOnUiThread(() =>
                                                       {
                                                           cs.Toasts.aToast.ShowToast(this,
                                                           /*"Текущая ячейка забронирована со стороны учетной системы"*/Android.App.Application.Context.GetText(Resource.String.ForTypesDocuments_Moving_isReservationAreaCells_Message),
                                                           ToastLength.Short, true, 1);
                                                       });
                                                        cs.BaseClass.currentIdCell = null;
                                                        cs.BaseClass.currentNameCell = null;
                                                        this.SafeRunOnUiThread(() =>
                                                        {
                                                            cellNameTextView.Text = null;
                                                        });
                                                        return;
                                                        //добавил блок 11.03.2025
                                                    }
                                                    //КОНЕЦ добавил блок 11.03.2025
                                                }
                                            }
                                            //КОНЕЦ добавил блок 04.03.2025_#1
                                        }
                                        //КОНЕЦ добавил блок 03.02.2025

                                        //добавил 15.04.2025
                                        SpecialDocuMoventmentListProductsInCell(cells);

                                        //добавил блок #_6 21.02.2024
                                        //добавил 03.02.2025 в условие && cells!=null
                                        if (cs.BaseClass.currentIdCell.HasValue && cells != null)
                                        {
                                            if (cells.CellF == cs.BaseClass.currentIdCell.Value)
                                            {
                                                //добавил блок 16.12.2024
                                                if (cs.BaseClass.currentDocHead != null)
                                                {
                                                    if (cs.BaseClass.currentDocHead.DocType == 4
                                                    && cs.BaseClass.currentDocHead.SubDocType == "selling"
                                                    && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_SellingCheckBox", false)
                                                    && cs.BaseClass.currentDocHead.StateMovedFrom
                                                    && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false)
                                                    && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                    {
                                                        this.SafeRunOnUiThread(async () =>
                                                        {
                                                            //добавил 03.02.2025
                                                            cellNameTextView.Text = cells.Name;
                                                            if (dialogCell != null)
                                                            {
                                                                if (!dialogCell.isCell)
                                                                {
                                                                    await CloseDocumentMethodAsync();
                                                                }
                                                            }
                                                            else
                                                            {
                                                                await CloseDocumentMethodAsync();
                                                            }
                                                        });
                                                        return;
                                                    }
                                                }
                                                //КОНЕЦ добавил блок 16.12.2024
                                                return;
                                            }
                                        }
                                        //end добавил блок #_6 21.02.2024

                                        //добавил блок 14.06.2024
                                        //добавил условие 17.06.2024
                                        if (cs.BaseClass.currentDocHead != null)
                                        {
                                            //добавил условие 17.06.2024
                                            if (cs.BaseClass.currentDocHead.Field_4 != null)
                                            {
                                                //17.06.2024 заменил PlanOrNewDocu && cs.BaseClass.preferences.GetBoolean("ForTypesDocuments_Inventory_ProhibitingScanningOfOtherCells", false) на cs.BaseClass.currentDocHead?.Field_4?.ToLower() == "ProhibitingScanningOtherCells".ToLower()
                                                if (cs.BaseClass.currentDocHead.DocType == 3 /*&& PlanOrNewDocu*/ && cs.BaseClass.currentDocHead?.Field_4?.ToLower() == "ProhibitingScanningOtherCells".ToLower()/*cs.BaseClass.preferences.GetBoolean("ForTypesDocuments_Inventory_ProhibitingScanningOfOtherCells", false)*/)
                                                {
                                                    bool findcell34 = false;
                                                    if (ListDocDetailsArray.mOriginalValues != null)
                                                    {
                                                        if (ListDocDetailsArray.mOriginalValues.Count > 0)
                                                        {
                                                            foreach (var item32 in ListDocDetailsArray.mOriginalValues)
                                                            {
                                                                if (item32.CellF.HasValue)
                                                                {
                                                                    if (cells.CellF == item32.CellF)
                                                                    {
                                                                        findcell34 = true;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (!findcell34)
                                                    {
                                                        this.RunOnUiThread(() =>
                                                        {
                                                            cs.Toasts.aToast.ShowToast(this,
                                                            /*"Запрещен выбор ячеек, отсутствующих в плановом документе инвентаризации"*/Android.App.Application.Context.GetText(Resource.String.ForTypesDocuments_Inventory_ProhibitingScanningOfOtherCells_Message),
                                                            ToastLength.Short, true, 1);
                                                        });
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                        //end добавил блок 14.06.2024

                                        cs.BaseClass.currentIdCell = cells.CellF;
                                        //cs.BaseClass.currentNameCell = cells.BarcodeCell;
                                        //cs.DocDetails.SelectedCellDialog.LastEnteringCellName = cells.BarcodeCell;
                                        cs.BaseClass.currentNameCell = cells.Name;
                                        cs.DocDetails.SelectedCellDialog.LastEnteringCellName = cells.Name;
                                        cs.BaseClass.preferences.Edit().PutString("currentNameCell",
                                            cs.DocDetails.SelectedCellDialog.LastEnteringCellName).Apply();

                                        bool isCell = false;
                                        if (dialogCell == null)
                                        {
                                            isCell = false;
                                        }
                                        else
                                        {
                                            isCell = dialogCell.isCell;
                                        }

                                        //если открыт диалог выбора ячейки
                                        if (isCell)
                                        {
                                            if (dialogCell.selectedCellEditText.Visibility == ViewStates.Visible)
                                            {
                                                //dialogCell.selectedCellEditText.Text = barcode.Remove(0, maskaCell.Length);
                                                dialogCell.selectedCellEditText.Text = cells.Name;
                                                dialogCell.ButtonNext_Click(null, null);
                                            }

                                            if (dialogCell.selectedCellSpinner.Visibility == ViewStates.Visible)
                                            {
                                                for (int i = 0; i < dialogCell.selectedCellSpinner.Adapter.Count; i++)
                                                {
                                                    if (dialogCell.selectedCellSpinner.Adapter.GetItem(i).ToString() ==
                                                        barcode.Remove(0, maskaCell.Length))
                                                    {
                                                        dialogCell.selectedCellSpinner.SetSelection(i);
                                                        dialogCell.ButtonNext_Click(null, null);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //cellNameTextView.Text =
                                            //    "Текущая ячейка: " + barcode.Remove(0, maskaCell.Length);
                                            //cs.Toasts.aToast.ShowToast(this,
                                            //    "Выбрана ячейка " + barcode.Remove(0, maskaCell.Length),
                                            //    ToastLength.Long, true, 2);
                                            //semaphore = new System.Threading.Semaphore(1, 1);
                                            //semaphore?.WaitOne();
                                            this.RunOnUiThread(() =>
                                            {
                                                if (!cs.BaseClass.preferences.GetBoolean("ShowCellDialogCheckBox", false))
                                                {
                                                    CellNameLinearLayout.Visibility = ViewStates.Visible;
                                                    cellNameTextView.Visibility = ViewStates.Visible;
                                                }


                                                cellNameTextView.Text =
                                                    /*"Текущая ячейка: " +*/ cells.Name;

                                                //добавил блок 23.06.2025
                                                try
                                                {
                                                    //01.09.2025 заменил имя метода Count на Count_Ext
                                                    if (ListDocDetailsArray.ListData.Where(c => c.CellF == cs.BaseClass.currentIdCell).Count_Ext() > 0)
                                                    {
                                                        //КОНЕЦ добавил блок 23.06.2025
                                                        cs.Toasts.aToast.ShowToast(this,
                                                    /*"Выбрана ячейка"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message39) + " " + cells.Name,
                                                    ToastLength.Long, true, 2);
                                                        //добавил блок 23.06.2025
                                                    }
                                                    else
                                                    {
                                                        bool findDDD = false;
                                                        foreach (var item101 in ListDocDetailsArray.ListData)
                                                        {
                                                            if (item101.cells != null)
                                                            {
                                                                if (item101.cells.Count > 0)
                                                                {
                                                                    //01.09.2025 заменил имя метода Count на Count_Ext
                                                                    if (item101.cells.Where(c => c.CellF == cs.BaseClass.currentIdCell).Count_Ext() > 0)
                                                                    {
                                                                        findDDD = true;
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        if (findDDD)
                                                        {
                                                            cs.Toasts.aToast.ShowToast(this,
                                                    /*"Выбрана ячейка"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message39) + " " + cells.Name,
                                                    ToastLength.Long, true, 2);
                                                        }
                                                        else
                                                        {
                                                            cs.Toasts.aToast.ShowToast(this,
                                                            /*"Выбрана ячейка"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message39) + " " + cells.Name,
                                                            ToastLength.Long, true, 4);
                                                        }
                                                    }
                                                }
                                                catch (System.Exception ex)
                                                {
                                                    ex = ex;
                                                    cs.Toasts.aToast.ShowToast(this,
                                                    /*"Выбрана ячейка"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message39) + " " + cells.Name,
                                                    ToastLength.Long, true, 2);
                                                }
                                                //КОНЕЦ добавил блок 23.06.2025


                                                //new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                //                    .SetMessage(/*"Не удалось добавить ячейку в справочник"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message39))
                                                //                    .SetCancelable(false)
                                                //                    .SetTitle(/*"База данных"*/Android.App.Application.Context.GetText(Resource.String.DataBase))
                                                //                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK), (senderAlert, args) => { })
                                                //                    .Show();
                                                //semaphore?.Release();
                                            });

                                            //if (!edit)
                                            //{
                                            //    SortAndFilterFromDocu(RootLayout);
                                            //}
                                            //else
                                            //{
                                            if (sortAndFilterClassDocDetails.vremSortAndFilterFileModel.Filter_CurrentCell)
                                            {
                                                /*await*/
                                                SortAndFilterFromAdapter(RootLayout);
                                                //SortAndFilterFromDocu(RootLayout);
                                            }

                                            //}
                                            //SortAndFilterFromDocu(RootLayout);

                                            //добавил блок 16.12.2024
                                            if (cs.BaseClass.currentDocHead != null)
                                            {
                                                if (cs.BaseClass.currentDocHead.DocType == 4
                                                && cs.BaseClass.currentDocHead.SubDocType == "selling"
                                                && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_SellingCheckBox", false)
                                                && cs.BaseClass.currentDocHead.StateMovedFrom
                                                && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false)
                                                && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                {
                                                    this.SafeRunOnUiThread(async () =>
                                                    {
                                                        await CloseDocumentMethodAsync();
                                                    });
                                                    return;
                                                }
                                            }
                                            //КОНЕЦ добавил блок 16.12.2024
                                        }
                                        return;
                                    }
                                    #endregion
                                }
                            }
                        }

                        if (!isBarcodeCellFind)
                        {
                            if (cs.BaseClass.preferences.GetBoolean("FindCellCheckBox", false))
                            {
                                using (SmartStoreData.SourceDataBase dataBase = new SmartStoreData.SourceDataBase())
                                {
                                    dataBase.SourceDataBaseMethod(cs.BaseClass.pathDocs);
                                    if (dataBase.ContextBDExsists)
                                    {
                                        dataBase.dataContext.generateLogExceptionProcess = this;
                                        //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                        var cell = dataBase.dataContext.Cell.AsNoTracking().Where(c => c.BarcodeCell == barcode)
                                            .FirstOrDefault_Ext();
                                        if (cell != null)
                                        {
                                            //добавил блок #_3 21.02.2024
                                            if (cs.BaseClass.OpenHandCard)
                                            {
                                                return;
                                            }
                                            if (productDialog != null)
                                            {
                                                if (productDialog.isOpened)
                                                {
                                                    return;
                                                }
                                            }
                                            //end добавил блок #_3 21.02.2024

                                            #region если фокус на компоненте поиска
                                            //условие добавлено 22.01.2024
                                            if (searchView.HasFocus)
                                            {
                                                isBarcodeCellFind = true;
                                                barcodeFindComponent = cell.Name;
                                                //добавил блок 03.02.2025
                                                if (cs.BaseClass.currentDocHead != null && cell != null)
                                                {
                                                    if (!cell.isPackingArea && cs.BaseClass.currentDocHead.DocType == 4
                                                            && cs.BaseClass.currentDocHead.SubDocType == "selling"
                                                            && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_SellingCheckBox", false)
                                                            && cs.BaseClass.currentDocHead.StateMovedFrom
                                                            && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false)
                                                            && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                    {
                                                        this.RunOnUiThread(() =>
                                                        {
                                                            cs.Toasts.aToast.ShowToast(this,
                                                            /*"Можно добавлять только ячейки зоны упаковки"*/Android.App.Application.Context.GetText(Resource.String.ForTypesDocuments_Moving_isPackingAreaCells_Message),
                                                            ToastLength.Short, true, 1);
                                                        });
                                                        //cs.BaseClass.currentIdCell = null;
                                                        //cs.BaseClass.currentNameCell = null;
                                                        //cellNameTextView.Text = null;
                                                        barcodeFindComponent_Insert = false;
                                                    }
                                                    //добавил блок 04.03.2025_#1
                                                    else
                                                    {
                                                        if (cell.isPackingArea && cell.isReservation && cs.BaseClass.currentDocHead.DocType == 4
                                                            && cs.BaseClass.currentDocHead.SubDocType == "selling"
                                                            && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_SellingCheckBox", false)
                                                            && cs.BaseClass.currentDocHead.StateMovedFrom
                                                            && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false)
                                                            && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                        {
                                                            //добавил блок 11.03.2025
                                                            bool breakOperation = cs.BaseClass.BreakTekCellReservedControl(cell);
                                                            if (breakOperation)
                                                            {
                                                                //КОНЕЦ добавил блок 11.03.2025
                                                                this.RunOnUiThread(() =>
                                                            {
                                                                cs.Toasts.aToast.ShowToast(this,
                                                                /*"Текущая ячейка забронирована со стороны учетной системы"*/Android.App.Application.Context.GetText(Resource.String.ForTypesDocuments_Moving_isReservationAreaCells_Message),
                                                                ToastLength.Short, true, 1);
                                                            });
                                                                //cs.BaseClass.currentIdCell = null;
                                                                //cs.BaseClass.currentNameCell = null;
                                                                //cellNameTextView.Text = null;
                                                                barcodeFindComponent_Insert = false;
                                                                //добавил блок 11.03.2025
                                                            }
                                                            //КОНЕЦ добавил блок 11.03.2025
                                                        }
                                                    }
                                                    //КОНЕЦ добавил блок 04.03.2025_#1
                                                }
                                                //КОНЕЦ добавил блок 03.02.2025

                                            }
                                            #endregion
                                            #region иначе, если фокус не на компоненте поиска
                                            else
                                            {
                                                //добавил блок 14.06.2024
                                                //добавил условие 17.06.2024
                                                if (cs.BaseClass.currentDocHead != null)
                                                {
                                                    //добавил условие 17.06.2024
                                                    if (cs.BaseClass.currentDocHead.Field_4 != null)
                                                    {
                                                        //17.06.2024 заменил PlanOrNewDocu && cs.BaseClass.preferences.GetBoolean("ForTypesDocuments_Inventory_ProhibitingScanningOfOtherCells", false) на cs.BaseClass.currentDocHead?.Field_4?.ToLower() == "ProhibitingScanningOtherCells".ToLower()
                                                        if (cs.BaseClass.currentDocHead.DocType == 3 && cs.BaseClass.currentDocHead?.Field_4?.ToLower() == "ProhibitingScanningOtherCells".ToLower()/*PlanOrNewDocu && cs.BaseClass.preferences.GetBoolean("ForTypesDocuments_Inventory_ProhibitingScanningOfOtherCells", false)*/)
                                                        {
                                                            bool findcell34 = false;
                                                            if (ListDocDetailsArray.mOriginalValues != null)
                                                            {
                                                                if (ListDocDetailsArray.mOriginalValues.Count > 0)
                                                                {
                                                                    foreach (var item32 in ListDocDetailsArray.mOriginalValues)
                                                                    {
                                                                        if (item32.CellF.HasValue)
                                                                        {
                                                                            if (cell.CellF == item32.CellF)
                                                                            {
                                                                                findcell34 = true;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            if (!findcell34)
                                                            {
                                                                this.RunOnUiThread(() =>
                                                                {
                                                                    cs.Toasts.aToast.ShowToast(this,
                                                                    /*"Запрещен выбор ячеек, отсутствующих в плановом документе инвентаризации"*/Android.App.Application.Context.GetText(Resource.String.ForTypesDocuments_Inventory_ProhibitingScanningOfOtherCells_Message),
                                                                    ToastLength.Short, true, 1);
                                                                });
                                                                return;
                                                            }
                                                        }
                                                    }
                                                }
                                                //end добавил блок 14.06.2024

                                                //добавил блок 03.02.2025
                                                if (cs.BaseClass.currentDocHead != null && cell != null)
                                                {
                                                    if (!cell.isPackingArea && cs.BaseClass.currentDocHead.DocType == 4
                                                            && cs.BaseClass.currentDocHead.SubDocType == "selling"
                                                            && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_SellingCheckBox", false)
                                                            && cs.BaseClass.currentDocHead.StateMovedFrom
                                                            && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false)
                                                            && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                    {
                                                        this.RunOnUiThread(() =>
                                                        {
                                                            cs.Toasts.aToast.ShowToast(this,
                                                            /*"Можно добавлять только ячейки зоны упаковки"*/Android.App.Application.Context.GetText(Resource.String.ForTypesDocuments_Moving_isPackingAreaCells_Message),
                                                            ToastLength.Short, true, 1);
                                                        });
                                                        cs.BaseClass.currentIdCell = null;
                                                        cs.BaseClass.currentNameCell = null;
                                                        cellNameTextView.Text = null;
                                                        return;
                                                    }
                                                    //добавил блок 04.03.2025_#1
                                                    else
                                                    {
                                                        if (cell.isPackingArea && cell.isReservation && cs.BaseClass.currentDocHead.DocType == 4
                                                            && cs.BaseClass.currentDocHead.SubDocType == "selling"
                                                            && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_SellingCheckBox", false)
                                                            && cs.BaseClass.currentDocHead.StateMovedFrom
                                                            && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false)
                                                            && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                        {
                                                            //добавил блок 11.03.2025
                                                            bool breakOperation = cs.BaseClass.BreakTekCellReservedControl(cell);
                                                            if (breakOperation)
                                                            {
                                                                //КОНЕЦ добавил блок 11.03.2025

                                                                this.RunOnUiThread(() =>
                                                            {
                                                                cs.Toasts.aToast.ShowToast(this,
                                                                /*"Текущая ячейка забронирована со стороны учетной системы"*/Android.App.Application.Context.GetText(Resource.String.ForTypesDocuments_Moving_isReservationAreaCells_Message),
                                                                ToastLength.Short, true, 1);
                                                            });
                                                                cs.BaseClass.currentIdCell = null;
                                                                cs.BaseClass.currentNameCell = null;
                                                                //добавил блок 26.06.2025
                                                                this.RunOnUiThread(() =>
                                                                {
                                                                    //КОНЕЦ добавил блок 26.06.2025
                                                                    cellNameTextView.Text = null;
                                                                    //добавил блок 26.06.2025
                                                                });
                                                                //КОНЕЦ добавил блок 26.06.2025
                                                                return;
                                                                //добавил блок 11.03.2025
                                                            }
                                                            //КОНЕЦ добавил блок 11.03.2025
                                                        }
                                                    }
                                                    //КОНЕЦ добавил блок 04.03.2025_#1
                                                }
                                                //КОНЕЦ добавил блок 03.02.2025

                                                //добавил 15.04.2025
                                                SpecialDocuMoventmentListProductsInCell(cell);

                                                //добавил блок #_6 21.02.2024
                                                if (cs.BaseClass.currentIdCell.HasValue)
                                                {
                                                    if (cell.CellF == cs.BaseClass.currentIdCell.Value)
                                                    {
                                                        //добавил блок 16.12.2024
                                                        if (cs.BaseClass.currentDocHead != null)
                                                        {
                                                            if (cs.BaseClass.currentDocHead.DocType == 4
                                                            && cs.BaseClass.currentDocHead.SubDocType == "selling"
                                                            && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_SellingCheckBox", false)
                                                            && cs.BaseClass.currentDocHead.StateMovedFrom
                                                            && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false)
                                                            && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                            {
                                                                this.SafeRunOnUiThread(async () =>
                                                                {
                                                                    //добавил 03.02.2025
                                                                    cellNameTextView.Text = cell.Name;
                                                                    if (dialogCell != null)
                                                                    {
                                                                        if (!dialogCell.isCell)
                                                                        {
                                                                            await CloseDocumentMethodAsync();
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        await CloseDocumentMethodAsync();
                                                                    }
                                                                });
                                                                return;
                                                            }
                                                        }
                                                        //КОНЕЦ добавил блок 16.12.2024
                                                        return;
                                                    }
                                                }
                                                //end добавил блок #_6 21.02.2024

                                                cs.BaseClass.currentIdCell = cell.CellF;
                                                cs.BaseClass.currentNameCell = cell.Name;
                                                cs.DocDetails.SelectedCellDialog.LastEnteringCellName = cell.Name;



                                                bool isCell = false;
                                                if (dialogCell == null)
                                                {
                                                    isCell = false;
                                                }
                                                else
                                                {
                                                    isCell = dialogCell.isCell;
                                                }

                                                //если открыт диалог выбора ячейки
                                                if (isCell)
                                                {
                                                    //добавил блок 26.06.2025
                                                    this.RunOnUiThread(() =>
                                                    {
                                                        //КОНЕЦ добавил блок 26.06.2025
                                                        if (dialogCell.selectedCellEditText.Visibility == ViewStates.Visible)
                                                        {
                                                            //dialogCell.selectedCellEditText.Text = barcode.Remove(0, maskaCell.Length);
                                                            dialogCell.selectedCellEditText.Text = cell.Name;
                                                            //dialogCell.ButtonNext_Click(null, null);
                                                        }

                                                        if (dialogCell.selectedCellSpinner.Visibility == ViewStates.Visible)
                                                        {
                                                            for (int i = 0; i < dialogCell.selectedCellSpinner.Adapter.Count; i++)
                                                            {
                                                                if (dialogCell.selectedCellSpinner.Adapter.GetItem(i).ToString() ==
                                                                    barcode)
                                                                {
                                                                    dialogCell.selectedCellSpinner.SetSelection(i);
                                                                    dialogCell.ButtonNext_Click(null, null);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        //добавил блок 26.06.2025
                                                    });
                                                    //КОНЕЦ добавил блок 26.06.2025
                                                }
                                                else
                                                {
                                                    this.RunOnUiThread(() =>
                                                    {
                                                        if (!cs.BaseClass.preferences.GetBoolean("ShowCellDialogCheckBox", false))
                                                        {
                                                            cellNameTextView.Visibility = ViewStates.Visible;
                                                            CellNameLinearLayout.Visibility = ViewStates.Visible;
                                                        }

                                                        //new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                        //            .SetMessage(/*"Не удалось добавить ячейку в справочник"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message39))
                                                        //            .SetCancelable(false)
                                                        //            .SetTitle(/*"База данных"*/Android.App.Application.Context.GetText(Resource.String.Yes))
                                                        //            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK), (senderAlert, args) => { })
                                                        //            .Show(); 

                                                        cs.BaseClass.currentIdCell = cell.CellF;
                                                        //cs.BaseClass.currentNameCell = cells.BarcodeCell;
                                                        //cs.DocDetails.SelectedCellDialog.LastEnteringCellName = cells.BarcodeCell;
                                                        cs.BaseClass.currentNameCell = cell.Name;
                                                        cs.DocDetails.SelectedCellDialog.LastEnteringCellName = cell.Name;
                                                        cs.BaseClass.preferences.Edit().PutString("currentNameCell",
                                                            cs.DocDetails.SelectedCellDialog.LastEnteringCellName).Apply();

                                                        cellNameTextView.Text =
                                                        /*"Текущая ячейка: " + */cell.Name;

                                                        //добавил блок 23.06.2025
                                                        try
                                                        {
                                                            //01.09.2025 заменил имя метода Count на Count_Ext
                                                            if (ListDocDetailsArray.ListData.Where(c => c.CellF == cs.BaseClass.currentIdCell).Count_Ext() > 0)
                                                            {
                                                                //КОНЕЦ добавил блок 23.06.2025
                                                                cs.Toasts.aToast.ShowToast(this,
                                                            /*"Выбрана ячейка"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message39) + " " + cell.Name,
                                                            ToastLength.Long, true, 2);
                                                                //добавил блок 23.06.2025
                                                            }
                                                            else
                                                            {
                                                                bool findDDD = false;
                                                                foreach (var item101 in ListDocDetailsArray.ListData)
                                                                {
                                                                    if (item101.cells != null)
                                                                    {
                                                                        if (item101.cells.Count > 0)
                                                                        {
                                                                            //01.09.2025 заменил имя метода Count на Count_Ext
                                                                            if (item101.cells.Where(c => c.CellF == cs.BaseClass.currentIdCell).Count_Ext() > 0)
                                                                            {
                                                                                findDDD = true;
                                                                                break;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                if (findDDD)
                                                                {
                                                                    cs.Toasts.aToast.ShowToast(this,
                                                            /*"Выбрана ячейка"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message39) + " " + cell.Name,
                                                            ToastLength.Long, true, 2);
                                                                }
                                                                else
                                                                {
                                                                    cs.Toasts.aToast.ShowToast(this,
                                                                    /*"Выбрана ячейка"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message39) + " " + cell.Name,
                                                                    ToastLength.Long, true, 4);
                                                                }
                                                            }
                                                        }
                                                        catch (System.Exception ex)
                                                        {
                                                            ex = ex;
                                                            cs.Toasts.aToast.ShowToast(this,
                                                            /*"Выбрана ячейка"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message39) + " " + cell.Name,
                                                            ToastLength.Long, true, 2);
                                                        }
                                                        //КОНЕЦ добавил блок 23.06.2025

                                                        //if (cs.BaseClass.preferences.GetBoolean("ShowDocDetails", false))
                                                        //{
                                                        if (sortAndFilterClassDocDetails.vremSortAndFilterFileModel.Filter_CurrentCell)
                                                        {
                                                            SortAndFilterFromAdapter(RootLayout);
                                                        }
                                                        //}
                                                        //SortAndFilterFromDocu(RootLayout);

                                                        //добавил блок 16.12.2024
                                                        if (cs.BaseClass.currentDocHead != null)
                                                        {
                                                            if (cs.BaseClass.currentDocHead.DocType == 4
                                                            && cs.BaseClass.currentDocHead.SubDocType == "selling"
                                                            && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_SellingCheckBox", false)
                                                            && cs.BaseClass.currentDocHead.StateMovedFrom
                                                            && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false)
                                                            && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                            {
                                                                this.SafeRunOnUiThread(async () =>
                                                                {
                                                                    await CloseDocumentMethodAsync();
                                                                });
                                                                return;
                                                            }
                                                        }
                                                        //КОНЕЦ добавил блок 16.12.2024
                                                    });

                                                }

                                                return;
                                            }
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        cs.AlerrtDialogs.MessageNotAvailableContextDataBase.ShowDialog(this, "$33");
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region если штрихкод GS1-128

                    CountAddGS1_128 = null;
                    shelfLifeAddGS1_128 = null;
                    seriesLifeAddGS1_128 = null;
                    BarcodeFindGS1_128 = false;
                    ScanGS1_128 = false;
                    if ((cs.BaseClass.preferences.GetBoolean("WeightUsed", false)) &&
                        (cs.BaseClass.preferences.GetBoolean("Weight_GS1_128", false)))
                    {
                        var resultModelGS1_128 = cs.Parser.EAN128Parser.Parse(barcode);
                        if (!string.IsNullOrWhiteSpace(resultModelGS1_128.Barcode))
                        {
                            ScanGS1_128 = true;
                            using (SmartStoreData.SourceDataBase dataBase = new SmartStoreData.SourceDataBase())
                            {
                                dataBase.SourceDataBaseMethod(cs.BaseClass.pathDocs);
                                if (dataBase.ContextBDExsists)
                                {
                                    dataBase.dataContext.generateLogExceptionProcess = this;
                                    var realBarcode = dataBase.dataContext.Barcode.AsNoTracking()
                                        //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                        .Where(c => c.Feature4 == resultModelGS1_128.Barcode).FirstOrDefault_Ext();
                                    if (realBarcode != null)
                                    {
                                        barcode = realBarcode.BarcodeName;
                                        BarcodeFindGS1_128 = true;
                                        if (resultModelGS1_128.Count.HasValue)
                                        {
                                            CountAddGS1_128 = resultModelGS1_128.Count.Value;
                                            shelfLifeAddGS1_128 = resultModelGS1_128.shelfLife;
                                            seriesLifeAddGS1_128 = resultModelGS1_128.series;
                                        }
                                        else
                                        {
                                            CountAddGS1_128 = -1;
                                            shelfLifeAddGS1_128 = resultModelGS1_128.shelfLife;
                                            seriesLifeAddGS1_128 = resultModelGS1_128.series;
                                        }
                                    }
                                }
                                else
                                {
                                    cs.AlerrtDialogs.MessageNotAvailableContextDataBase.ShowDialog(this, "$17");
                                    return;
                                }
                            }

                        }
                        else
                        {

                            CountAddGS1_128 = null;
                            shelfLifeAddGS1_128 = resultModelGS1_128.shelfLife;
                            seriesLifeAddGS1_128 = resultModelGS1_128.series;

                            ScanGS1_128 = false;
                            //resultModelGS1_128 = null;
                            BarcodeFindGS1_128 = false;

                            //было
                            //if (resultModelGS1_128.Count.HasValue)
                            //{
                            //    //было
                            //    ScanGS1_128 = true;


                            //    //было
                            //    if (resultModelGS1_128.Count.Value > 0)
                            //    {
                            //        CountAddGS1_128 = resultModelGS1_128.Count.Value;
                            //        shelfLifeAddGS1_128 = resultModelGS1_128.shelfLife;
                            //        seriesLifeAddGS1_128 = resultModelGS1_128.series;
                            //    }
                            //    else
                            //    {
                            //        CountAddGS1_128 = null;
                            //        shelfLifeAddGS1_128 = resultModelGS1_128.shelfLife;
                            //        seriesLifeAddGS1_128 = resultModelGS1_128.series;
                            //    }

                            //    BarcodeFindGS1_128 = false;
                            //}
                        }

                    }

                    #endregion

                    #region если фокус у элемента поиска
                    if (searchView.HasFocus)
                    {
                        //isBarcodeCellFind = isBarcodeCellFind;

                        //закоментировал условие 22.01.2024 т.к. не работал поиск при повторном сканировании, если фокус на єлементе поиска
                        //if (string.IsNullOrWhiteSpace(searchView.Query))
                        //{
                        SVPDroidLib.Keyboard.hideKeyboard(this);
                        if (string.IsNullOrEmpty(barcodeFindComponent))
                        {
                            searchView.SetQuery(barcode, true);
                        }
                        else
                        {
                            //добавил условие 03.02.2025
                            if (barcodeFindComponent_Insert)
                            {
                                searchView.SetQuery(barcodeFindComponent, true);
                            }
                        }
                        return;
                        //}
                    }
                    #endregion

                    #region идентификация серийных штрихкодов
                    if (cs.BaseClass.preferences.GetBoolean("Serial_Barcodes", false))
                    {
                        bool exsistSerialBarcode = false;

                        string prefix_SerialBarcodes = cs.BaseClass.preferences.GetString("Prefix_SerialBarcodes", "");
                        if (string.IsNullOrWhiteSpace(prefix_SerialBarcodes))
                        {
                            exsistSerialBarcode = true;
                        }
                        else
                        {
                            #region читаем префиксы штрихкодов
                            try
                            {
                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                var tre = prefix_SerialBarcodes.Split(',').ToList_Ext();
                                for (int i = 0; i < tre.Count; i++)
                                {
                                    if (!string.IsNullOrWhiteSpace(tre[i]))
                                    {
                                        if (barcode.StartsWith(tre[i].Trim(' ')))
                                        {
                                            exsistSerialBarcode = true;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                cs.Toasts.aToast.ShowToast(this, /*"Не удалось прочитать префиксы штрихкодов для серийный штрихкодов"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message41),
                                                        ToastLength.Long, true, 1);
                                return;
                            }

                            #endregion
                        }

                        if (exsistSerialBarcode)
                        {
                            for (int i = 0; i < cs.DocDetails.ListDocDetailsArray.mOriginalValues.Count; i++)
                            {
                                //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                var nayd_barcode = cs.DocDetails.ListDocDetailsArray.mOriginalValues[i].ScanHistory.Where(c => c.BarcodeName == barcode).FirstOrDefault_Ext();
                                if (nayd_barcode != null)
                                {
                                    /*"Штрихкод {0} уже был ранее привязан к документу"*/
                                    //23.01.2026_#5 закоментировал
                                    //cs.Toasts.aToast.ShowToast(this, string.Format(Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message42), string.Empty /*cs.BaseClass.statusBarcodeModel.CompositeBarcode*/),
                                    //                ToastLength.Long, true, 1);
                                    //23.01.2026_#5 заменил cs.BaseClass.statusBarcodeModel.CompositeBarcode на string.Empty
                                    //по просьбе клиента Булочник (Запорожье) и изменил формулироваку сообщения на
                                    //Штрихкод был ранее привязан к одному из документов текущего типа {0}

                                    //23.01.2026_#5 добавил блок
                                    using (cs.SnackBar snackBar = new cs.SnackBar())
                                    {
                                        snackBar.ShowText(container, string.Format(Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message42), string.Empty /*cs.BaseClass.statusBarcodeModel.CompositeBarcode*/),
                                            5000, false, true, 1);
                                    }
                                    //КОНЕЦ 23.01.2026_#5 добавил блок
                                    return;
                                }

                            }
                        }
                    }
                    #endregion

                    #region если выключена настройка для ячеек показа диалога при открытии документа

                    //добавил 03.07.2024 'cs.BaseClass.currentDocHead.DocType!=7 && cs.BaseClass.currentDocHead.DocType != 8' в условие 
                    if (cs.BaseClass.currentDocHead.DocType != 7 && cs.BaseClass.currentDocHead.DocType != 8 && !cs.BaseClass.preferences.GetBoolean("ShowCellDialogCheckBox", false) && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                    {
                        if (cs.BaseClass.currentIdCell == 0 || cs.BaseClass.currentNameCell == null)
                        {
                            //добавил блок 18.06.2024
                            //ВАЖНО!!!! когда сделаем метод асинхроннім то нужно исзменить вызов метода
                            //InfoCellsFromProductBarc на await InfoCellsFromProductBarcAsync
                            if (!InfoCellsFromProductBarc(barcode))
                            {
                                return;
                            }
                            //КОНЕЦ добавил блок 18.06.2024
                            cs.Toasts.aToast.ShowToast(this, /*"Укажите штрихкод ячейки"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_SelectedCellDialog_Message10),
                                                        ToastLength.Long, true, 3);
                            return;
                        }
                    }
                    #endregion

                    //добавил блок 16.10.2025
                    #region проверяем является ли штрихкод заводским штрихкодом товара
                    //добавил блок 04.05.2026_#1
                    var isFacBarc = IsFactoryBarcode(ref barcode);
                    if (isFacBarc.isFactoryBarcode && isFacBarc.stop)
                    {
                        return;
                    }
                    //КОНЕЦ добавил блок 04.05.2026_#1

                    #region закоментировал блок 04.05.2026_#1
                    //var resFactoryBarcode = cs.Helpers.FactoryBarcodeHelpers.Instance.HasFactoryBarcode(barcode);
                    //if (resFactoryBarcode.result)
                    //{
                    //    using (SmartStoreData.SourceDataBase dataBase =
                    //                            new SmartStoreData.SourceDataBase(cs.BaseClass.pathDocs))
                    //    {
                    //        if (dataBase.ContextBDExsists)
                    //        {
                    //            dataBase.dataContext.generateLogExceptionProcess = this;

                    //            //добавил блок 20.10.2025
                    //            if (!cs.Helpers.FactoryBarcodeHelpers.Instance.CheckFactoryBarcode(this, dataBase, barcode)) return;
                    //            //КОНЕЦ добавил блок 20.10.2025

                    //            var resArr = resFactoryBarcode.listAttribute.ToArray();
                    //            var barcPr = dataBase.dataContext.Barcode.AsNoTracking().Include(c=>c.Good).Where(c => resArr.Contains(c.FactoryBarcodeSign)).ToList_Ext();
                    //            if (barcPr != null)
                    //            {
                    //                if (barcPr.Count > 0)
                    //                {
                    //                    var longest = barcPr
                    //                                  .Where(c => c.FactoryBarcodeSign != null)
                    //                                  .OrderByDescending(c => c.FactoryBarcodeSign.Length)
                    //                                  .FirstOrDefault_Ext();
                    //                    if (longest.Good != null)
                    //                    {
                    //                        cs.Helpers.FactoryBarcodeHelpers.Instance.originalBarcode = barcode;
                    //                        cs.Helpers.FactoryBarcodeHelpers.Instance.resFactoryBarcode = true;
                    //                        barcode = longest.BarcodeName;
                    //                        if (cs.AutoTestClass.FabricBarcodeTest)
                    //                        {
                    //                            cs.BaseClass.preferences.Edit()
                    //                                    .PutString("HasFactoryBarcode_current", "true, " + cs.Helpers.FactoryBarcodeHelpers.Instance.originalBarcode)
                    //                                    .Apply();
                    //                        }
                    //                        //var ghj = cs.Helpers.FactoryBarcodeHelpers.originalBarcode;
                    //                        //ghj = ghj;
                    //                    }
                    //                    else
                    //                    {
                    //                        //выводим сообщение В базе данных отсутствует информация о товаре
                    //                        //this?.RunOnUiThread(() =>
                    //                        //{
                    //                            cs.Toasts.aToast.ShowToast(this, /*"В базе данных отсутствует информация о товаре"*/this.GetText(Resource.String.FactoryBarcodes_Message2), ToastLength.Short, true);

                    //                        //});
                    //                        cs.Helpers.FactoryBarcodeHelpers.Instance.originalBarcode = null;
                    //                        cs.Helpers.FactoryBarcodeHelpers.Instance.resFactoryBarcode = false;
                    //                        if (cs.AutoTestClass.FabricBarcodeTest)
                    //                        {
                    //                            cs.BaseClass.preferences.Edit()
                    //                                            .PutString("HasFactoryBarcode_current", "false, no good")
                    //                                            .Apply();
                    //                        }
                    //                        return;
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    //выводим сообщение В базе данных отсутствует информация о товаре
                    //                    //this?.RunOnUiThread(() =>
                    //                    //{
                    //                        cs.Toasts.aToast.ShowToast(this, /*"В базе данных отсутствует информация о товаре"*/this.GetText(Resource.String.FactoryBarcodes_Message2), ToastLength.Short, true);

                    //                    //});
                    //                    if (cs.AutoTestClass.FabricBarcodeTest)
                    //                    {
                    //                        cs.BaseClass.preferences.Edit()
                    //                                        .PutString("HasFactoryBarcode_current", "false, no barcode")
                    //                                        .Apply();
                    //                    }
                    //                    cs.Helpers.FactoryBarcodeHelpers.Instance.originalBarcode = null;
                    //                    cs.Helpers.FactoryBarcodeHelpers.Instance.resFactoryBarcode = false;
                    //                    return;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                //выводим сообщение В базе данных отсутствует информация о товаре c указанным признаком штрихкода заводского номера
                    //                //this?.RunOnUiThread(() =>
                    //                //{
                    //                    cs.Toasts.aToast.ShowToast(this, /*"В базе данных отсутствует информация о товаре c указанным признаком штрихкода заводского номера"*/this.GetText(Resource.String.FactoryBarcodes_Message1), ToastLength.Short, true);
                    //                //});
                    //                cs.Helpers.FactoryBarcodeHelpers.Instance.originalBarcode = null;
                    //                cs.Helpers.FactoryBarcodeHelpers.Instance.resFactoryBarcode = false;
                    //                return;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            cs.AlerrtDialogs.MessageNotAvailableContextDataBase.ShowDialog(this, "$183");
                    //            return;
                    //        }
                    //    }
                    //}
                    #endregion КОНЕЦ закоментировал блок 04.05.2026_#1
                    #endregion
                    //КОНЕЦ добавил блок 16.10.2025



                    switch (cs.BaseClass.currentDocHead.DocType)
                    {
                        #region все другие документы

                        default:
                            //нужно ли отменить продолжение выполнения кода
                            bool cancelOperationConfirm = false;

                            #region если нужно подтвержать сканером

                            if (cs.BaseClass.preferences.GetBoolean("ConfirmCardProductScanerCheckBox", true))
                            {

                                if (cs.BaseClass.ConfirmCardProductScaner)
                                {
                                    //добавил блок 29.05.2024
                                    if (cs.BaseClass.preferences.GetBoolean("ForTypesDocuments_Incoming_ExpirationDate", false) && cs.BaseClass.currentDocHead.DocType == 1 && !ExpirationDateDialog.ApplyDate && cs.DocDetails.CardProductDialog.currentGood?.Field_2 == ExpirationDateDialog.MarkerExpirationDate)
                                    {
                                        return;
                                    }
                                    //КОНЕЦ добавил блок 29.05.2024
                                    //25.01.2026_#2 добавил блок
                                    if (Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.IsWaveAssemblyDocument())
                                    {
                                        //в документах волновой сборки не подтверждаем сканером
                                        return;
                                    }
                                    //КОНЕЦ 25.01.2026_#2 добавил блок
                                    try
                                    {
                                        cancelOperationConfirm =
                                            !cs.DialogFragments.BaseDialogFragment.dialogFragment/*((ListDocDetailsActivity)this).productDialog*/.ConfirmEnter(false);

                                        if (CountAddGS1_128.HasValue && !cancelOperationConfirm && !BarcodeFindGS1_128)
                                        {
                                            if (CountAddGS1_128.Value > 0)
                                            {
                                                return;
                                            }
                                        }

                                        //if (!cs.DialogFragments.BaseDialogFragment.dialogFragment.ButtonOK(null, null)
                                        var buttonokresult = /*await*/ cs.DialogFragments.BaseDialogFragment.dialogFragment./*((ListDocDetailsActivity)this).productDialog.*/ButtonOK(null, null);
                                        if (!buttonokresult)
                                        {
                                            return;
                                        }

                                        //добавил блок 08.08.2024
                                        if (productDialog != null)
                                        {
                                            try
                                            {
                                                productDialog.Dismiss();
                                            }
                                            catch (System.Exception e)
                                            {
                                                e = e;
                                            }
                                            if (productDialog != null)
                                            {
                                                try
                                                {
                                                    productDialog.Dispose();
                                                }
                                                catch (System.Exception e)
                                                {
                                                    e = e;
                                                }
                                            }
                                        }
                                        //КОНЕЦ добавил блок 08.08.2024
                                        productDialog = null;

                                    }
                                    catch (System.NullReferenceException ex)
                                    {
                                        using (GenerateLogException generateLog = new GenerateLogException())
                                        {
                                            generateLog.InfoException(ex, "ListDocDetailsActivity.ScanerInit_BarcodeDataEvent.ConfirmCardProductScaner.NullReferenceException", cs.BaseClass.FreeMemoryMessage());
                                        }
                                    }
                                    catch (System.Exception ex)
                                    {
                                        ex = ex;
                                        using (GenerateLogException generateLog = new GenerateLogException())
                                        {
                                            generateLog.InfoException(ex, "ListDocDetailsActivity.ScanerInit_BarcodeDataEvent.ConfirmCardProductScaner", cs.BaseClass.FreeMemoryMessage());
                                        }
                                    }
                                }
                            }

                            #endregion

                            //если документ еще не был завершен
                            if ((cs.BaseClass.currentDocHead.DocStatus < 2) || (cs.BaseClass.currentDocHead.DocStatus > 3))
                            {
                                #region отсеивающиеся штрихкоды

                                if (barcode != null)
                                {
                                    if (barcode.Length > 2)
                                    {
                                        if (barcode.Substring(0, 3) == "AT+")
                                        {
                                            return;
                                        }
                                    }
                                }

                                #endregion

                                this.RunOnUiThread(() =>
                                {
                                    //если при подтверждении сканером были обнаружены ошибки 
                                    if (cancelOperationConfirm) return;

                                    #region определяем, штучный ли товар или весовой
                                    //04.04.2025 закоментировал  cs.StatusBarcodeModel
                                    //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                    /*cs.StatusBarcodeModel*/
                                    //добавил условие 19.09.2025_#4
                                    if (!NotReadStatusBarcode)
                                    {
                                        cs.BaseClass.statusBarcodeModel = cs.BaseClass.StatusBarcode(barcode);
                                    }
                                    #endregion
                                    //04.04.2025 закоментировал statusBarcodeModel
                                    //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                    if (/*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel.ErrorWeightBarcode)
                                    {
                                        //добавил 09.09.2025 this.RunOnUiThread(() =>
                                        //this.RunOnUiThread(() =>
                                        //{
                                        cs.AlerrtDialogs.AlertDialogs.AlertDialogBase(this,
                                        //04.04.2025 закоментировал statusBarcodeModel
                                        //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                        cs.BaseClass.statusBarcodeModel.ErrorWeightBarcodeMessage, /*"Разбор штрихкода"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message4), /*09.09.2025 заменил false на true*/true);
                                        //});
                                        return;
                                    }

                                    //добавил блок 25.09.2025
                                    if (cs.DocDetails.Menu.UnPackScanDialog.Instance.isActiveDialog)
                                    {
                                        if (cs.BaseClass.statusBarcodeModel.StatusCompositeBarcodeBatchAccountingModel)
                                        {
                                            //добавил блок 02.10.2025_#1
                                            //var ttt = cs.BaseClass.statusBarcodeModel;
                                            //ttt = ttt;
                                            cs.DocDetails.Menu.UnPackScanDialog.Instance.ResetErrorBarcode();
                                            if (cs.BaseClass.statusBarcodeModel.ErrorParseBarcodeBatchAccounting)
                                            {
                                                cs.DocDetails.Menu.UnPackScanDialog.Instance.ErrorBarcode(cs.BaseClass.statusBarcodeModel.ErrorParseBarcodeBatchAccountingMessage);
                                            }
                                            else
                                            {
                                                if (cs.BaseClass.statusBarcodeModel.compositeBarcodeModel.count == 1 && cs.BaseClass.statusBarcodeModel.compositeBarcodeModel.view_pack == "2")
                                                {
                                                    cs.DocDetails.Menu.UnPackScanDialog.Instance.ErrorBarcode(cs.BaseClass.GetLocaleText(
                                                        "Вказаний вид пакування товару не може бути розпакований",
                                                        "Указанный вид упаковки товара не может быть распакован",
                                                        "The specified type of product packaging cannot be unpacked",
                                                        "Podany typ opakowania produktu nie może być rozpakowany"));
                                                }
                                                else
                                                {
                                                    //КОНЕЦ добавил блок 02.10.2025_#1
                                                    cs.DocDetails.Menu.UnPackScanDialog.Instance.HideQuestion();
                                                    //добавил блок 02.10.2025_#1
                                                }

                                            }
                                            //КОНЕЦ добавил блок 02.10.2025_#1
                                            return;
                                        }
                                        else
                                        {
                                            //добавил блок 02.10.2025_#1
                                            cs.DocDetails.Menu.UnPackScanDialog.Instance.ErrorBarcode(cs.BaseClass.GetLocaleText(
                                        "Не вдалося розібрати штрихкод",
                                        "Не удалось разобрать штрихкод",
                                        "Unable to parse the barcode",
                                        "Nie można przeanalizować kodu kreskowego"));
                                            //КОНЕЦ добавил блок 02.10.2025_#1
                                            return;
                                        }
                                    }
                                    //КОНЕЦ добавил блок 25.09.2025

                                    //добавил блок 19.09.2025_#4
                                    //04.04.2025 закоментировал statusBarcodeModel
                                    //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                    if (/*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel.ShowQuestionDialog &&
                                    !NotReadStatusBarcode)
                                    {
                                        this.SafeRunOnUiThread(() =>
                                        {
                                            //cs.ScanerInit.ActiveScaner(false);
                                            _ = cs.Sound.NotificationSound.PlayNotificationAsync(this, "invalid");
                                            new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                .SetMessage(cs.BaseClass.statusBarcodeModel.ShowQuestionDialogMessage)
                                                .SetTitle(/*"Штрихкод"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message7))
                                                .SetCancelable(false)
                                                .SetPositiveButton(/*"Да"*/Android.App.Application.Context.GetText(Resource.String.Yes),
                                                    (senderAlert, args) =>
                                                    {
                                                        NotReadStatusBarcode = true;
                                                        if (cs.BaseClass.statusBarcodeModel.StatusCompositeBarcodeModel)
                                                        {
                                                            barcode = cs.BaseClass.statusBarcodeModel.compositeBarcodeModel.barc;
                                                            cs.BaseClass.statusBarcodeModel.StatusCompositeBarcodeModel = false;
                                                            cs.BaseClass.statusBarcodeModel.CompositeBarcode = null;
                                                        }
                                                        ScanerInit_BarcodeDataEvent(barcode, type);
                                                        NotReadStatusBarcode = false;
                                                        //cs.ScanerInit.ActiveScaner(true);
                                                    })
                                                .SetNegativeButton(/*"Нет"*/Android.App.Application.Context.GetText(Resource.String.No), (senderAlert, args) => { /*cs.ScanerInit.ActiveScaner(true);*/})
                                                .Show();
                                        });

                                        return;
                                    }
                                    //КОНЕЦ добавил блок 19.09.2025_#4

                                    //добавил блок 09.09.2025
                                    //04.04.2025 закоментировал statusBarcodeModel
                                    //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                    if (/*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel.ErrorParseBarcodeBatchAccounting)
                                    {
                                        //добавил 09.09.2025 this.RunOnUiThread(() =>
                                        //this.RunOnUiThread(() =>
                                        //{
                                        cs.AlerrtDialogs.AlertDialogs.AlertDialogBase(this,
                                        //04.04.2025 закоментировал statusBarcodeModel
                                        //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                        cs.BaseClass.statusBarcodeModel.ErrorParseBarcodeBatchAccountingMessage, /*"Разбор штрихкода"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message4), /*09.09.2025 заменил false на true*/true);
                                        //});
                                        return;
                                    }
                                    //КОНЕЦ добавил блок 09.09.2025

                                    bool isCell = false;
                                    if (dialogCell == null)
                                    {
                                        isCell = false;
                                    }
                                    else
                                    {
                                        isCell = dialogCell.isCell;
                                    }

                                    //если открыт диалог выбора ячейки
                                    if (isCell)
                                    {
                                        if (dialogCell.selectedCellEditText.Visibility == ViewStates.Visible)
                                        {
                                            dialogCell.selectedCellEditText.Text =
                                                cs.DocDetails.SelectedCellDialog.LastEnteringCellName;
                                            //dialogCell.selectedCellEditText.Text = barcode;
                                        }

                                        if (dialogCell.selectedCellSpinner.Visibility == ViewStates.Visible)
                                        {
                                            for (int i = 0; i < dialogCell.selectedCellSpinner.Adapter.Count; i++)
                                            {
                                                if (dialogCell.selectedCellSpinner.Adapter.GetItem(i).ToString() ==
                                                    cs.DocDetails.SelectedCellDialog.LastEnteringCellName)
                                                {
                                                    dialogCell.selectedCellSpinner.SetSelection(i);
                                                    break;
                                                }

                                                //if (dialogCell.selectedCellSpinner.Adapter.GetItem(i).ToString() == barcode)
                                                //{
                                                //    dialogCell.selectedCellSpinner.SetSelection(i);
                                                //    break;
                                                //}
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (floatingActionMenu.IsOpened)
                                        {
                                            //закрывем меню
                                            floatingActionMenu.Close(true);
                                        }

                                        try
                                        {
                                            container.Alpha = 0;
                                        }
                                        catch
                                        {
                                        }

                                        CurrentBarcode = barcode;
                                        try
                                        {
                                            using (SmartStoreData.SourceDataBase dataBase =
                                                new SmartStoreData.SourceDataBase(/*cs.BaseClass.pathDocs*/))
                                            {
                                                dataBase.SourceDataBaseMethod(cs.BaseClass.pathDocs);
                                                if (dataBase.ContextBDExsists)
                                                {
                                                    dataBase.dataContext.generateLogExceptionProcess = this;
                                                    #region ограничение в демо режиме

                                                    //if (cs.BaseClass.demoMode)
                                                    //{
                                                    //    if (dataBase.dataContext.ScanHistory/*.ToList()*/.Count_Ext() >= cs.BaseClass.DemoModeCountScanHistory)
                                                    //    {
                                                    //        this.RunOnUiThread(() =>
                                                    //        {
                                                    //            cs.ScanerInit.ActiveScaner(false);
                                                    //            new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                    //                                        .SetMessage("Ограничение демострационного режима")
                                                    //                                        .SetTitle("Demo режим")
                                                    //                                        .SetCancelable(false)
                                                    //                                        .SetPositiveButton("OK", (senderAlert, args) => { ((AlertDialog)senderAlert).Dismiss(); })
                                                    //                                        .Show()
                                                    //                                        .DismissEvent += (sender2, e2) => { cs.ScanerInit.ActiveScaner(true); };
                                                    //        });
                                                    //        return;
                                                    //    }
                                                    //}

                                                    #endregion

                                                    #region если товар штучный
                                                    //04.04.2025 закоментировал statusBarcodeModel
                                                    //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                    if (/*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel.StatusProduct == false)
                                                    {
                                                        //04.04.2025 добавил условие
                                                        //09.09.2025 добавил в условие && !cs.BaseClass.statusBarcodeModel.ErrorParseBarcodeBatchAccounting
                                                        if (!cs.BaseClass.statusBarcodeModel.StatusCompositeBarcodeModel && !cs.BaseClass.statusBarcodeModel.StatusCompositeBarcodeBatchAccountingModel)
                                                        {
                                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                            List<SmartStoreData.Barcode> barcodes = dataBase.dataContext.Barcode.AsNoTracking()
                                                                .Include(c => c.Good).Where(c => c.BarcodeName == CurrentBarcode)
                                                                .ToList_Ext();

                                                            #region если штрихкод не найден

                                                            if (barcodes.Count == 0)
                                                            {
                                                                this.RunOnUiThread(() =>
                                                                {
                                                                    cs.ScanerInit.ActiveScaner(false);
                                                                    cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                                    new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                        .SetMessage(/*"Штрихкод"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message7) + " " + CurrentBarcode + " " +/*"не найден"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message8))
                                                                        .SetTitle(/*"Штрихкод"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message7))
                                                                        .SetCancelable(false)
                                                                        .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                            (senderAlert, args) =>
                                                                            {
                                                                                cs.ScanerInit.ActiveScaner(true);
                                                                            })
                                                                        .Show();
                                                                });

                                                                return;
                                                            }

                                                            #endregion

                                                            #region если штрихкод найден

                                                            else
                                                            {
                                                                if (barcodes.Count == 1)
                                                                {
                                                                    //List<SmartStoreData.Good> goods = dataBase.dataContext.Good
                                                                    //    .Where(c => c.GoodF.EndsWith(barcodes[0].GoodF))
                                                                    //    .ToList_Ext();
                                                                    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                    List<SmartStoreData.Good> goods = dataBase.dataContext.Good.AsNoTracking()
                                                                        .Where(c => c.GoodF == barcodes[0].GoodF).ToList_Ext();
                                                                    if (goods.Count == 0)
                                                                    {
                                                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                        goods = dataBase.dataContext.Good.AsNoTracking()
                                                                            .Where(c => c.GoodF.EndsWith(barcodes[0].GoodF))
                                                                            .ToList_Ext();
                                                                    }

                                                                    foreach (var item in goods)
                                                                    {
                                                                        if (item.GoodF == barcodes[0].GoodF)
                                                                        {
                                                                            try
                                                                            {
                                                                                goods = new List<SmartStoreData.Good>() { item };
                                                                            }
                                                                            catch (System.Data.SqlClient.SqlException ex)
                                                                            {
                                                                                cs.AlerrtDialogs.MessageNotAvailableDataBaseShow
                                                                                    alertDialogNotAvailable =
                                                                                        new cs.AlerrtDialogs.
                                                                                            MessageNotAvailableDataBaseShow(this,
                                                                                                ex.GetBaseException().Message);
                                                                                return;
                                                                            }
                                                                        }

                                                                        break;
                                                                    }

                                                                    barcodeScanOperation(dataBase, goods, barcodes, CurrentBarcode,
                                                                        //04.04.2025 закоментировал statusBarcodeModel
                                                                        //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                        /*statusBarcodeModel*/ cs.BaseClass.statusBarcodeModel);
                                                                }
                                                                else
                                                                {
                                                                    if (!cs.BaseClass.HendEnter)
                                                                    {
                                                                        //01.09.2025 заменил имя метода Select на Select_Ext
                                                                        string[] vremgoods = barcodes.Select_Ext(c => c.Good.Name)
                                                                            .Distinct().ToArray();
                                                                        string EnterGoodF = null;
                                                                        var activityvrem = this;
                                                                        cs.ScanerInit.ActiveScaner(false);
                                                                        var alertDialog = new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                            .SetItems(vremgoods, (sender2, args) =>
                                                                            {
                                                                                using (SmartStoreData.SourceDataBase dataBase2 =
                                                                                    new SmartStoreData.SourceDataBase(/*cs.BaseClass.pathDocs*/))
                                                                                {
                                                                                    dataBase.SourceDataBaseMethod(cs.BaseClass.pathDocs);
                                                                                    if (dataBase2.ContextBDExsists)
                                                                                    {
                                                                                        dataBase2.dataContext.generateLogExceptionProcess = this;
                                                                                        try
                                                                                        {
                                                                                            EnterGoodF = dataBase2.dataContext.Good.AsNoTracking()
                                                                                                .Where(c =>
                                                                                                    c.Name == vremgoods[args.Which])
                                                                                                //01.09.2025 заменил имя метода First на First_Ext
                                                                                                .First_Ext().GoodF;
                                                                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                                            List<SmartStoreData.Good> goods =
                                                                                                dataBase2.dataContext.Good.AsNoTracking().Where(c =>
                                                                                                        c.Name == vremgoods[args.Which])
                                                                                                    .ToList_Ext();
                                                                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                                            barcodes = barcodes.Where(c =>
                                                                                                c.GoodF == EnterGoodF).ToList_Ext();
                                                                                            barcodeScanOperation(dataBase2, goods,
                                                                                                barcodes, CurrentBarcode,
                                                                                                //04.04.2025 закоментировал statusBarcodeModel
                                                                                                //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                                                /*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel);
                                                                                        }
                                                                                        catch (System.Data.SqlClient.SqlException ex)
                                                                                        {
                                                                                            cs.AlerrtDialogs.
                                                                                                MessageNotAvailableDataBaseShow
                                                                                                alertDialogNotAvailable =
                                                                                                    new cs.AlerrtDialogs.
                                                                                                        MessageNotAvailableDataBaseShow(
                                                                                                            this,
                                                                                                            ex.GetBaseException()
                                                                                                                .Message);
                                                                                            return;
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        cs.ScanerInit.ActiveScaner(false);
                                                                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                                               .SetMessage(/*"База данных не доступна"*/this.GetText(Resource.String.DatabaseNotAvailable))
                                                                                               .SetCancelable(false)
                                                                                               .SetTitle(/*"База данных"*/this.GetText(Resource.String.DataBase))
                                                                                               .SetPositiveButton(/*"Закрыть"*/this.GetText(Resource.String.Close), (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                                                               .Show();
                                                                                        return;
                                                                                    }
                                                                                }
                                                                            })
                                                                            .SetCancelable(false)
                                                                            .SetTitle(/*"Выберите товар из списка"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_FindBarcodeOrCodeProductDialog_Message8))
                                                                            .Show();
                                                                        alertDialog.DismissEvent += (sender5, e5) =>
                                                                        {
                                                                            cs.ScanerInit.ActiveScaner(true);
                                                                        };
                                                                    }
                                                                    else
                                                                    {
                                                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                        List<SmartStoreData.Good> goods = dataBase.dataContext.Good.AsNoTracking()
                                                                            .Where(c => c.GoodF.EndsWith(barcodes[0].GoodF))
                                                                            .ToList_Ext();
                                                                        barcodeScanOperation(dataBase, goods, barcodes,
                                                                            //04.04.2025 закоментировал statusBarcodeModel
                                                                            //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                            CurrentBarcode, /*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel);
                                                                    }
                                                                }
                                                            }

                                                            #endregion
                                                        }
                                                        //04.04.2025 добавил блок 
                                                        else
                                                        {
                                                            //добавил блок 02.07.2025
                                                            if (cs.BaseClass.preferences.GetBoolean("Serial_Barcodes", false))
                                                            {
                                                                //29.08.2025 заменил имя метода Count на Count_Ext
                                                                //закоментировал строку 12.01.2026
                                                                //var countCompBarc = dataBase.dataContext.ScanHistory.AsNoTracking().Where(c => c.CompositeBarcodeName == cs.BaseClass.statusBarcodeModel.CompositeBarcode).Count_Ext();
                                                                //добавил блок 12.01.2026
                                                                //ВАЖНО! Только для текущего типа документов
                                                                var countCompBarc = dataBase.dataContext.DocHead.AsNoTracking()
                                                                             .Include(h => h.DocDetails)
                                                                                 .ThenInclude(d => d.ScanHistory)
                                                                             .Where(h => h.DocType == cs.BaseClass.currentDocHead.DocType && h.DocDetails
                                                                                 .Any(d => d.ScanHistory
                                                                                     .Any(s => s.CompositeBarcodeName == cs.BaseClass.statusBarcodeModel.CompositeBarcode)))
                                                                             .Count_Ext();
                                                                //КОНЕЦ добавил блок 12.01.2026
                                                                if (countCompBarc > 0)
                                                                {
                                                                    this.RunOnUiThread(() =>
                                                                        {
                                                                            /*"Штрихкод {0} уже был ранее привязан к документу"*/
                                                                            //23.01.2026_#5 закоментировал
                                                                            //cs.Toasts.aToast.ShowToast(this, string.Format(Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message42), string.Empty /*cs.BaseClass.statusBarcodeModel.CompositeBarcode*/),
                                                                            //                ToastLength.Long, true, 1);
                                                                            //23.01.2026_#5 заменил cs.BaseClass.statusBarcodeModel.CompositeBarcode на string.Empty
                                                                            //по просьбе клиента Булочник (Запорожье) и изменил формулироваку сообщения на
                                                                            //Штрихкод был ранее привязан к одному из документов текущего типа {0}

                                                                            //23.01.2026_#5 добавил блок
                                                                            using (cs.SnackBar snackBar = new cs.SnackBar())
                                                                            {
                                                                                snackBar.ShowText(container, string.Format(Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message42), string.Empty /*cs.BaseClass.statusBarcodeModel.CompositeBarcode*/),
                                                                                    5000, false, true, 1);
                                                                            }
                                                                            //КОНЕЦ 23.01.2026_#5 добавил блок
                                                                        });
                                                                    return;
                                                                }
                                                            }
                                                            //КОНЕЦ добавил блок 02.07.2025

                                                            List<SmartStoreData.Barcode> barcodes = new List<Barcode>();
                                                            if (!string.IsNullOrWhiteSpace(cs.BaseClass.statusBarcodeModel.compositeBarcodeModel.barc))
                                                            {
                                                                CurrentBarcode = cs.BaseClass.statusBarcodeModel.compositeBarcodeModel.barc;
                                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                barcodes = dataBase.dataContext.Barcode.AsNoTracking()
                                                                .Include(c => c.Good).Where(c => c.BarcodeName == cs.BaseClass.statusBarcodeModel.compositeBarcodeModel.barc)
                                                                .ToList_Ext();
                                                                if (barcodes.Count > 0)
                                                                {
                                                                    barcodes[0].Count = cs.BaseClass.statusBarcodeModel.compositeBarcodeModel.count;
                                                                }
                                                            }

                                                            List<SmartStoreData.Good> goods = new List<Good>();
                                                            if (!string.IsNullOrWhiteSpace(cs.BaseClass.statusBarcodeModel.compositeBarcodeModel.sku))
                                                            {
                                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                goods = dataBase.dataContext.Good.AsNoTracking()
                                                                        .Where(c => c.GoodF == cs.BaseClass.statusBarcodeModel.compositeBarcodeModel.sku).ToList_Ext();
                                                                if (goods.Count > 0)
                                                                {
                                                                    if (barcodes.Count == 0)
                                                                    {
                                                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                        barcodes = dataBase.dataContext.Barcode.AsNoTracking()
                                                                                   .Include(c => c.Good).Where(c => c.GoodF == goods[0].GoodF)
                                                                                   .ToList_Ext();
                                                                        if (barcodes.Count > 0)
                                                                        {
                                                                            CurrentBarcode = barcodes[0].BarcodeName;
                                                                            barcodes[0].Count = cs.BaseClass.statusBarcodeModel.compositeBarcodeModel.count;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (barcodes.Count > 0)
                                                                {
                                                                    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                    goods = dataBase.dataContext.Good.AsNoTracking()
                                                                            .Where(c => c.GoodF == barcodes[0].GoodF).ToList_Ext();
                                                                }
                                                            }

                                                            if (goods.Count > 0 && barcodes.Count > 0)
                                                            {
                                                                if (goods[0].GoodF != barcodes[0].GoodF)
                                                                {
                                                                    this.RunOnUiThread(() =>
                                                                    {
                                                                        cs.ScanerInit.ActiveScaner(false);
                                                                        cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                            .SetMessage(/*"Товар не привязан к указанному штрихкоду"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message1000))
                                                                            .SetTitle(/*"Штрихкод"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message7))
                                                                            .SetCancelable(false)
                                                                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                                (senderAlert, args) =>
                                                                                {
                                                                                    cs.ScanerInit.ActiveScaner(true);
                                                                                })
                                                                            .Show();
                                                                    });

                                                                    return;
                                                                }
                                                            }


                                                            //if (goods.Count > 0 || barcodes.Count > 0)
                                                            //{
                                                            if (goods.Count > 0 && barcodes.Count > 0)
                                                            {
                                                                //if (goods.Count > 0 && barcodes.Count > 0)
                                                                //{
                                                                //if(goods[0].GoodF!= barcodes[0].GoodF)
                                                                //{
                                                                //    this.RunOnUiThread(() =>
                                                                //    {
                                                                //        cs.ScanerInit.ActiveScaner(false);
                                                                //        cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                                //        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                //            .SetMessage(/*"Товар не привязан к указанному штрихкоду"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message1000))
                                                                //            .SetTitle(/*"Штрихкод"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message7))
                                                                //            .SetCancelable(false)
                                                                //            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                //                (senderAlert, args) =>
                                                                //                {
                                                                //                    cs.ScanerInit.ActiveScaner(true);
                                                                //                })
                                                                //            .Show();
                                                                //    });

                                                                //    return;
                                                                //}
                                                                //else
                                                                //{
                                                                barcodeScanOperation(dataBase, goods, barcodes, CurrentBarcode,
                                                                        cs.BaseClass.statusBarcodeModel);
                                                                //}
                                                                //}
                                                                //else
                                                                //{
                                                                //    //barcodeScanOperation(dataBase, goods, barcodes, CurrentBarcode,
                                                                //    //            statusBarcodeModel);
                                                                //}
                                                            }
                                                            else
                                                            {
                                                                this.RunOnUiThread(() =>
                                                                {
                                                                    cs.ScanerInit.ActiveScaner(false);
                                                                    cs.Sound.NotificationSound.PlayNotification(this, "invalid");

                                                                    if (goods.Count == 0)
                                                                    {
                                                                        //string messageAlertD = string.Format("Ни одного товара, соответствующего штрихкоду {0} не было найдено", CurrentBarcode);
                                                                        string messageAlertD = string.Format(Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message47), CurrentBarcode);
                                                                        //04.04.2025 закоментировал statusBarcodeModel
                                                                        //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                        if (/*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel.StatusProduct)
                                                                        {
                                                                            //messageAlertD = "Ни одного товара с кодом " + statusBarcodeModel.CodeProduct + " не было найдено";
                                                                            //04.04.2025 закоментировал statusBarcodeModel
                                                                            //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                            messageAlertD = string.Format(Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message48), /*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel.CodeProduct);
                                                                        }

                                                                        this.RunOnUiThread(() =>
                                                                        {
                                                                            cs.ScanerInit.ActiveScaner(false);
                                                                            cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                                            new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                                .SetMessage(messageAlertD)
                                                                                .SetTitle(/*"Штрихкод"*/Android.App.Application.Context.GetText(Resource.String.Layout_CardProductLandscape_BarcodeFull))
                                                                                .SetCancelable(false)
                                                                                .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK), (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                                                .Show();
                                                                        });
                                                                        return;
                                                                    }

                                                                    if (barcodes.Count == 0)
                                                                    {
                                                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                            .SetMessage(/*"Штрихкод"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message7) + " " + CurrentBarcode + " " +/*"не найден"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message8))
                                                                            .SetTitle(/*"Штрихкод"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message7))
                                                                            .SetCancelable(false)
                                                                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                                (senderAlert, args) =>
                                                                                {
                                                                                    cs.ScanerInit.ActiveScaner(true);
                                                                                })
                                                                            .Show();
                                                                    }

                                                                });

                                                                return;
                                                            }
                                                        }
                                                        //КОНЕЦ 04.04.2025 добавил блок 
                                                    }

                                                    #endregion

                                                    #region если товар весовой

                                                    else
                                                    {

                                                        //добавил блок 02.07.2025
                                                        if (cs.BaseClass.statusBarcodeModel.StatusCompositeBarcodeModel && cs.BaseClass.preferences.GetBoolean("Serial_Barcodes", false))
                                                        {
                                                            //29.08.2025 заменил имя метода Count на Count_Ext
                                                            //закоментировал строку 12.01.2026
                                                            //var countCompBarc = dataBase.dataContext.ScanHistory.AsNoTracking().Where(c => c.CompositeBarcodeName == cs.BaseClass.statusBarcodeModel.CompositeBarcode).Count_Ext();
                                                            //добавил блок 12.01.2026
                                                            //ВАЖНО! Только для текущего типа документов
                                                            var countCompBarc = dataBase.dataContext.DocHead.AsNoTracking()
                                                                         .Include(h => h.DocDetails)
                                                                             .ThenInclude(d => d.ScanHistory)
                                                                         .Where(h => h.DocType == cs.BaseClass.currentDocHead.DocType && h.DocDetails
                                                                             .Any(d => d.ScanHistory
                                                                                 .Any(s => s.CompositeBarcodeName == cs.BaseClass.statusBarcodeModel.CompositeBarcode)))
                                                                         .Count_Ext();
                                                            //КОНЕЦ добавил блок 12.01.2026
                                                            if (countCompBarc > 0)
                                                            {
                                                                this.RunOnUiThread(() =>
                                                                {
                                                                    /*"Штрихкод {0} уже был ранее привязан к документу"*/
                                                                    //23.01.2026_#5 закоментировал
                                                                    //cs.Toasts.aToast.ShowToast(this, string.Format(Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message42), string.Empty /*cs.BaseClass.statusBarcodeModel.CompositeBarcode*/),
                                                                    //                ToastLength.Long, true, 1);
                                                                    //23.01.2026_#5 заменил cs.BaseClass.statusBarcodeModel.CompositeBarcode на string.Empty
                                                                    //по просьбе клиента Булочник (Запорожье) и изменил формулироваку сообщения на
                                                                    //Штрихкод был ранее привязан к одному из документов текущего типа {0}

                                                                    //23.01.2026_#5 добавил блок
                                                                    using (cs.SnackBar snackBar = new cs.SnackBar())
                                                                    {
                                                                        snackBar.ShowText(container, string.Format(Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message42), string.Empty /*cs.BaseClass.statusBarcodeModel.CompositeBarcode*/),
                                                                            5000, false, true, 1);
                                                                    }
                                                                    //КОНЕЦ 23.01.2026_#5 добавил блок
                                                                });
                                                                return;
                                                            }
                                                        }
                                                        //КОНЕЦ добавил блок 02.07.2025

                                                        List<SmartStoreData.Good> goods = new List<SmartStoreData.Good>();
                                                        List<SmartStoreData.Barcode> barcodes = null;
                                                        switch (cs.BaseClass.preferences.GetString("WeightCodeProductOrArtikul",
                                                            "1"))
                                                        {
                                                            case "1":
                                                                //Trello Весовой штрихкод
                                                                //04.04.2025 закоментировал statusBarcodeModel
                                                                //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                goods = dataBase.dataContext.Good.AsNoTracking().Where(c =>
                                                                    c.GoodF ==/*.EndsWith(*//*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel.CodeProduct/*)*/).ToList_Ext();
                                                                if (goods.Count > 0)
                                                                {
                                                                    //04.04.2025 закоментировал statusBarcodeModel
                                                                    //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                    /*statusBarcodeModel*/
                                                                    //01.09.2025 заменил имя метода First на First_Ext
                                                                    cs.BaseClass.statusBarcodeModel.CodeProduct = goods.First_Ext().GoodF;
                                                                }

                                                                else
                                                                {
                                                                    goods = new List<SmartStoreData.Good>();
                                                                }

                                                                break;
                                                            case "2":
                                                                //Trello Весовой штрихкод
                                                                //04.04.2025 закоментировал statusBarcodeModel
                                                                //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                barcodes = dataBase.dataContext.Barcode.AsNoTracking().Include(c => c.Good)
                                                                    .Where(c => c.Code ==/*.EndsWith(*//*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel.CodeProduct/*)*/)
                                                                    .ToList_Ext();
                                                                if (barcodes.Count > 0)
                                                                {
                                                                    //01.09.2025 заменил имя метода First на First_Ext
                                                                    if (barcodes.First_Ext().Good != null)
                                                                    {
                                                                        goods = new List<SmartStoreData.Good>()
                                                                        //01.09.2025 заменил имя метода First на First_Ext
                                                                        {barcodes.First_Ext().Good};
                                                                    }
                                                                    //04.04.2025 закоментировал statusBarcodeModel
                                                                    //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                    /*statusBarcodeModel*/
                                                                    //01.09.2025 заменил имя метода First на First_Ext
                                                                    cs.BaseClass.statusBarcodeModel.CodeProduct =
                                                                        barcodes.First_Ext().Good.GoodF;
                                                                }
                                                                else
                                                                {
                                                                    barcodes = null;
                                                                    goods = new List<SmartStoreData.Good>();
                                                                }

                                                                break;
                                                            case "3":
                                                                var bbb3 = cs.BaseClass.statusBarcodeModel;
                                                                bbb3 = bbb3;

                                                                //Trello Весовой штрихкод
                                                                //04.04.2025 закоментировал statusBarcodeModel
                                                                //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                goods = dataBase.dataContext.Good.Include(c => c.Barcode).AsNoTracking().Where(c =>
                                                                    c.GoodF ==/*.EndsWith(*//*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel.CodeProduct/*)*/).ToList_Ext();
                                                                barcodes = null;
                                                                if (goods.Count == 0)
                                                                {
                                                                    //Trello Весовой штрихкод
                                                                    //04.04.2025 закоментировал statusBarcodeModel
                                                                    //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                    barcodes = dataBase.dataContext.Barcode.AsNoTracking().Include(c => c.Good)
                                                                        .Where(c => c.Code ==/*.EndsWith(*//*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel
                                                                            .CodeProduct/*)*/).ToList_Ext();
                                                                    if (barcodes.Count > 0)
                                                                    {
                                                                        //01.09.2025 заменил имя метода First на First_Ext
                                                                        goods.Add(barcodes.First_Ext().Good);
                                                                        if (barcodes.First_Ext().Good != null)
                                                                        {
                                                                            //04.04.2025 закоментировал statusBarcodeModel
                                                                            //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                            //01.09.2025 заменил имя метода First на First_Ext
                                                                            /*statusBarcodeModel*/
                                                                            cs.BaseClass.statusBarcodeModel.CodeProduct =
                                                                                barcodes.First_Ext().Good.GoodF;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    //04.04.2025 закоментировал statusBarcodeModel
                                                                    //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                                    /*statusBarcodeModel*/
                                                                    cs.BaseClass.statusBarcodeModel.CodeProduct = goods[0].GoodF;
                                                                    //добавил блок 04.08.2025_#1
                                                                    if (goods[0].Barcode != null)
                                                                    {
                                                                        if (goods[0].Barcode.Count > 0)
                                                                        {
                                                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                            barcodes = goods[0].Barcode.ToList_Ext();
                                                                        }
                                                                    }
                                                                    var tttt3 = cs.BaseClass.statusBarcodeModel;
                                                                    tttt3 = tttt3;
                                                                    //КОНЕЦ добавил блок 04.08.2025_#1
                                                                }

                                                                break;
                                                        }
                                                        //List<SmartStoreData.Good> goods = dataBase.dataContext.Good.Where(c => c.GoodF.Substring(c.GoodF.Length - statusBarcodeModel.CodeProduct.Length - 1, statusBarcodeModel.CodeProduct.Length) == statusBarcodeModel.CodeProduct).ToList_Ext();
                                                        //04.04.2025 закоментировал statusBarcodeModel
                                                        //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                        barcodeScanOperation(dataBase, goods, barcodes, CurrentBarcode,
                                                            /*statusBarcodeModel*/cs.BaseClass.statusBarcodeModel);
                                                    }

                                                    #endregion
                                                }
                                                else
                                                {
                                                    cs.AlerrtDialogs.MessageNotAvailableContextDataBase.ShowDialog(this, "$18");
                                                    return;
                                                }
                                            }
                                        }
                                        catch (System.Data.SqlClient.SqlException ex)
                                        {
                                            cs.AlerrtDialogs.MessageNotAvailableDataBaseShow alertDialogNotAvailable =
                                                new cs.AlerrtDialogs.MessageNotAvailableDataBaseShow(this,
                                                    ex.GetBaseException().Message);
                                            return;
                                        }
                                        finally
                                        {
                                            cs.BaseClass.GC();
                                        }
                                    }
                                });
                            }
                            else
                            {
                                cs.Toasts.aToast.ShowToast(this, /*"Изменение данных запрещено"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message43), ToastLength.Short, true);
                            }

                            break;

                        #endregion

                        #region документ сбор штрихкодов

                        case 7:
                            try
                            {
                                using (SmartStoreData.SourceDataBase dataBase =
                                    new SmartStoreData.SourceDataBase(/*cs.BaseClass.pathDocs*/))
                                {
                                    dataBase.SourceDataBaseMethod(cs.BaseClass.pathDocs);
                                    if (dataBase.ContextBDExsists)
                                    {
                                        dataBase.dataContext.generateLogExceptionProcess = this;
                                        #region ограничение в демо режиме

                                        //if (cs.BaseClass.demoMode)
                                        //{
                                        //    if (dataBase.dataContext.ScanHistory/*.ToList()*/.Count_Ext() >= cs.BaseClass.DemoModeCountScanHistory)
                                        //    {
                                        //        this.RunOnUiThread(() =>
                                        //        {
                                        //            cs.ScanerInit.ActiveScaner(false);
                                        //            new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                        //                                        .SetMessage("Ограничение демострационного режима")
                                        //                                        .SetTitle("Demo режим")
                                        //                                        .SetCancelable(false)
                                        //                                        .SetPositiveButton("OK", (senderAlert, args) => { ((AlertDialog)senderAlert).Dismiss(); })
                                        //                                        .Show()
                                        //                                        .DismissEvent += (sender2, e2) => { cs.ScanerInit.ActiveScaner(true); };
                                        //        });
                                        //        return;
                                        //    }
                                        //}

                                        #endregion

                                        //был ли найден элемент в списке
                                        bool Finded = false;

                                        #region нахожу, если таковой имеется, элемент в списке

                                        for (int i = 0; i < adapter.Count; i++)
                                        {
                                            SmartStoreData.DocDetails docDetails = adapter.GetItem(i);
                                            if (barcode == docDetails.Spec_comment)
                                            {
                                                adapter.current = docDetails;
                                                posIndex = i;
                                                Finded = true;
                                                break;
                                            }
                                        }

                                        #endregion

                                        if (!Finded)
                                        {
                                            #region нахожу, если таковой имеется, в документе (из БД)

                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            var vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                                .Where(c => c.Spec_comment == barcode).ToList_Ext();
                                            if (vremgoodsList.Count > 0)
                                            {
                                                adapter.current = vremgoodsList[0];
                                                Finded = true;
                                            }

                                            #endregion
                                        }

                                        //если найден в документе
                                        List<SmartStoreData.DocDetails> list = new List<SmartStoreData.DocDetails>();
                                        if (Finded)
                                        {
                                            var details = adapter.current;
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            list = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                                .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)
                                                .Where(c => c.Spec_comment == details.Spec_comment).ToList_Ext();
                                            if (list.Count > 0)
                                            {
                                                //double sumCountReal = list.Sum(c => c.Count_Real) + 1;
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                var scanhistory = list.First_Ext().ScanHistory;
                                                double sumCountReal = 0;
                                                foreach (var item2 in scanhistory)
                                                {
                                                    sumCountReal += item2.Count;
                                                }

                                                sumCountReal += 1;
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                list.First_Ext().Count_Real = sumCountReal;
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                list.First_Ext().UpdatedFromTSD = true;
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                list.First_Ext().CreateDate = DateTime.Now.Ticks;
                                                adapter.current.Count_Real = sumCountReal;
                                                adapter.current.UpdatedFromTSD = true;
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                adapter.current.CreateDate = list.First_Ext().CreateDate;
                                                adapter.Update();
                                            }
                                        }
                                        //если не найден в документе
                                        else
                                        {
                                            adapter.current = new SmartStoreData.DocDetails();
                                            adapter.current.Bad_price = false;
                                            if (cs.BaseClass.currentIdCell > 0)
                                            {
                                                adapter.current.CellF = cs.BaseClass.currentIdCell;
                                            }

                                            //флаг ручного изменения истории
                                            adapter.current.Change_history = false;
                                            adapter.current.Count_Doc = 0;

                                            adapter.current.Count_Real = 1;
                                            adapter.current.CreateDate = DateTime.Now.Ticks;
                                            adapter.current.DocHeadF = cs.BaseClass.currentDocHead.DocHeadF;
                                            adapter.current.GoodF = "999999999999999999999999999999";
                                            adapter.current.Good = dataBase.dataContext.Good/*.AsNoTracking()*/
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                .Where(c => c.GoodF == adapter.current.GoodF).First_Ext();
                                            ////////adapter.current.Have_comment = false;
                                            ////////adapter.current.Have_spec_comment = true;
                                            adapter.current.Spec_comment = barcode;
                                            //ручной ввод
                                            adapter.current.Hend_enter = cs.BaseClass.HendEnter;
                                            adapter.current.DocDetailsF = Guid.NewGuid();
                                            //////adapter.current.Price = 0;

                                            ////////adapter.current.SummDoc = null;
                                            ////////adapter.current.SummReal = null;
                                            adapter.current.UpdatedFromTSD = true;
                                            adapter.current.UpdateFrom1C = false;
                                            adapter.current.UserF = cs.BaseClass.currentIdUser;
                                            if (SmartStoreData.SourceDataBase.LocalDB == SmartStoreData.TypeDatabaseEnum.local)
                                            {
                                                adapter.current.id = 1;
                                                try
                                                {
                                                    LinqExtensions.WriteErrorToFile = false;
                                                    //29.08.2025 заменил .Max на .Max_Ext
                                                    adapter.current.id = dataBase.dataContext.DocDetails.AsNoTracking().Max_Ext(c => c.id);
                                                    LinqExtensions.WriteErrorToFile = true;
                                                }
                                                catch
                                                {
                                                }
                                            }

                                            dataBase.dataContext.DocDetails.Add(adapter.current);
                                        }

                                        try
                                        {
                                            int y = dataBase.dataContext.SaveChanges();

                                            if (y != 1)
                                            {
                                                if (list.Count > 0)
                                                {
                                                    this.RunOnUiThread(() =>
                                                    {
                                                        cs.ScanerInit.ActiveScaner(false);
                                                        cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                            .SetCancelable(false)
                                                            .SetMessage(
                                                                /*"Не удалось обновить информацию о товаре со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message35) + " " +
                                                                barcode + " " +/*"в текущем документе"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message32))
                                                            .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                            .Show();
                                                    });
                                                    return;
                                                }
                                                else
                                                {
                                                    this.RunOnUiThread(() =>
                                                    {
                                                        cs.ScanerInit.ActiveScaner(false);
                                                        cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                            .SetMessage(
                                                                /*"Не удалось добавить товар со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message40) + " " + barcode +
                                                                " " +/*"в текущий документ"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message41))
                                                            .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                            .SetCancelable(false)
                                                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                            .Show();
                                                    });
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                if (!Finded)
                                                {
                                                    adapter.Insert(adapter.current, 0);
                                                }

                                                if (list.Count > 0)
                                                {
                                                    using (cs.ScanHistory history = new cs.ScanHistory())
                                                    {
                                                        //01.09.2025 заменил имя метода First на First_Ext
                                                        var trew = /*await*/ history.AddHistory/*Async*/(this, list.First_Ext(), 1, barcode, 0);
                                                        if (trew = false)
                                                        {
                                                            this.RunOnUiThread(() =>
                                                            {
                                                                cs.ScanerInit.ActiveScaner(false);
                                                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                    .SetMessage(
                                                                        /*"Не удалось обновить историю сканирований товара со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message37) + " " +
                                                                        barcode + " " +/*"для текущего документа"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message36))
                                                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                                    .SetCancelable(false)
                                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                        (senderAlert, args) =>
                                                                        {
                                                                            cs.ScanerInit.ActiveScaner(true);
                                                                        })
                                                                    .Show();
                                                            });
                                                        }
                                                        else
                                                        {
                                                            adapter.current.ScanHistory.Add(cs.ScanHistory.history);
                                                            adapter.current.Count_Real += 1;
                                                            adapter.Update();
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    using (cs.ScanHistory history = new cs.ScanHistory())
                                                    {
                                                        var tre = /*await*/ history.AddHistory/*Async*/(this, adapter.current, 1, barcode, 0);
                                                        if (tre == false)
                                                        {
                                                            this.RunOnUiThread(() =>
                                                            {
                                                                cs.ScanerInit.ActiveScaner(false);
                                                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");

                                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                    .SetMessage(
                                                                        /*"Не удалось обновить историю сканирований товара со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message37) + " " +
                                                                        barcode + " " +/*"для текущего документа"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message36))
                                                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                                    .SetCancelable(false)
                                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                        (senderAlert, args) =>
                                                                        {
                                                                            cs.ScanerInit.ActiveScaner(true);
                                                                        })
                                                                    .Show();
                                                            });
                                                        }
                                                        else
                                                        {
                                                            adapter.current.ScanHistory.Add(cs.ScanHistory.history);
                                                            adapter.current.Count_Real += 1;
                                                            adapter.Update();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch (System.Exception ex)
                                        {
                                            ex = ex;
                                            this.RunOnUiThread(() =>
                                            {
                                                cs.ScanerInit.ActiveScaner(false);
                                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");

                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                    .SetMessage(
                                                        /*"Не удалось обновить/добавить информацию о товаре со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message44) + " " +
                                                        barcode + " " + /*"в текущем документе"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message32))
                                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                    .SetCancelable(false)
                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                        (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                    .Show();
                                            });
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        cs.AlerrtDialogs.MessageNotAvailableContextDataBase.ShowDialog(this, "$19");
                                        return;
                                    }
                                }
                            }
                            catch (System.Data.SqlClient.SqlException ex)
                            {
                                cs.AlerrtDialogs.MessageNotAvailableDataBaseShow alertDialogNotAvailable =
                                    new cs.AlerrtDialogs.MessageNotAvailableDataBaseShow(this,
                                        ex.GetBaseException().Message);
                                return;
                            }
                            //catch (System.Exception ex)
                            //{
                            //    ex = ex;
                            //}
                            finally
                            {
                                cs.BaseClass.GC();
                            }

                            break;

                        #endregion

                        #region документ сбор штрихкодов с характеристиками

                        case 8:
                            try
                            {
                                using (SmartStoreData.SourceDataBase dataBase =
                                    new SmartStoreData.SourceDataBase(/*cs.BaseClass.pathDocs*/))
                                {
                                    dataBase.SourceDataBaseMethod(cs.BaseClass.pathDocs);
                                    if (dataBase.ContextBDExsists)
                                    {
                                        dataBase.dataContext.generateLogExceptionProcess = this;
                                        #region ограничение в демо режиме

                                        //if (cs.BaseClass.demoMode)
                                        //{
                                        //    if (dataBase.dataContext.ScanHistory/*.ToList()*/.Count_Ext() >= cs.BaseClass.DemoModeCountScanHistory)
                                        //    {
                                        //        this.RunOnUiThread(() =>
                                        //        {
                                        //            cs.ScanerInit.ActiveScaner(false);
                                        //            new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                        //                                        .SetMessage("Ограничение демострационного режима")
                                        //                                        .SetTitle("Demo режим")
                                        //                                        .SetCancelable(false)
                                        //                                        .SetPositiveButton("OK", (senderAlert, args) => { ((AlertDialog)senderAlert).Dismiss(); })
                                        //                                        .Show()
                                        //                                        .DismissEvent += (sender2, e2) => { cs.ScanerInit.ActiveScaner(true); };
                                        //        });
                                        //        return;
                                        //    }
                                        //}

                                        #endregion

                                        //был ли найден элемент в списке
                                        bool Finded = false;

                                        if (charasteristik_BarcodeOne == null)
                                        {
                                            if (floatingActionMenu.IsOpened)
                                            {
                                                //закрывем меню
                                                floatingActionMenu.Close(true);
                                            }
                                            if (collectionOfBarcodesWithCharacteristicsDialog != null)
                                            {
                                                collectionOfBarcodesWithCharacteristicsDialog.Dismiss();
                                            }

                                            charasteristik_BarcodeOne = barcode;
                                            charasteristik_BarcodeTwo = null;
                                            cs.DocDetails.CollectionOfBarcodesWithCharacteristicsDialog.barcode = barcode;
                                            collectionOfBarcodesWithCharacteristicsDialog =
                                                (cs.DocDetails.CollectionOfBarcodesWithCharacteristicsDialog)
                                                new cs.DialogFragments.BaseDialogFragment().CreateDialog(this,
                                                    "@string/new_name_title_dialog",
                                                    cs.Enums.FragmentDialogTypes.Сбор_штрихкода_с_характеристиками, null,
                                                    adapter);
                                            collectionOfBarcodesWithCharacteristicsDialog.DismissEvent +=
                                                CollectionOfBarcodesWithCharacteristicsDialog_DismissEvent;

                                            return;
                                        }
                                        else
                                        {
                                            charasteristik_BarcodeTwo = barcode;
                                        }

                                        if (charasteristik_BarcodeOne == charasteristik_BarcodeTwo)
                                        {
                                            cs.Toasts.aToast.ShowToast(this,
                                                /*"Штрихкод товара не может соответствовать штрихкоду характеристики товара"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message45),
                                                ToastLength.Long, true);
                                            return;
                                        }

                                        string ConnectString = charasteristik_BarcodeOne + "^" + charasteristik_BarcodeTwo;

                                        charasteristik_BarcodeTwo = null;
                                        charasteristik_BarcodeOne = null;

                                        collectionOfBarcodesWithCharacteristicsDialog.Dismiss();

                                        #region нахожу, если таковой имеется, элемент в списке

                                        for (int i = 0; i < adapter.Count; i++)
                                        {
                                            SmartStoreData.DocDetails docDetails = adapter.GetItem(i);
                                            if (ConnectString == docDetails.Spec_comment)
                                            {
                                                adapter.current = docDetails;
                                                posIndex = i;
                                                Finded = true;
                                                break;
                                            }
                                        }

                                        #endregion

                                        if (!Finded)
                                        {
                                            #region нахожу, если таковой имеется, в документе (из БД)

                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            var vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                                .Where(c => c.Spec_comment == ConnectString).ToList_Ext();
                                            if (vremgoodsList.Count > 0)
                                            {
                                                adapter.current = vremgoodsList[0];
                                                Finded = true;
                                            }

                                            #endregion
                                        }

                                        //если найден в документе
                                        List<SmartStoreData.DocDetails> list = new List<SmartStoreData.DocDetails>();
                                        if (Finded)
                                        {
                                            var details = adapter.current;
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            list = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                                .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)
                                                .Where(c => c.Spec_comment == details.Spec_comment).ToList_Ext();
                                            if (list.Count > 0)
                                            {
                                                //double sumCountReal = list.Sum(c => c.Count_Real) + 1;
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                var scanhistory = list.First_Ext().ScanHistory;
                                                double sumCountReal = 0;
                                                foreach (var item2 in scanhistory)
                                                {
                                                    sumCountReal += item2.Count;
                                                }

                                                sumCountReal += 1;
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                list.First_Ext().Count_Real = sumCountReal;
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                list.First_Ext().UpdatedFromTSD = true;
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                list.First_Ext().CreateDate = DateTime.Now.Ticks;
                                                adapter.current.Count_Real = sumCountReal;
                                                adapter.current.UpdatedFromTSD = true;
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                adapter.current.CreateDate = list.First_Ext().CreateDate;
                                                adapter.Update();
                                            }
                                        }
                                        //если не найден в документе
                                        else
                                        {
                                            adapter.current = new SmartStoreData.DocDetails();
                                            adapter.current.Bad_price = false;
                                            if (cs.BaseClass.currentIdCell > 0)
                                            {
                                                adapter.current.CellF = cs.BaseClass.currentIdCell;
                                            }

                                            //флаг ручного изменения истории
                                            adapter.current.Change_history = false;
                                            adapter.current.Count_Doc = 0;

                                            adapter.current.Count_Real = 1;
                                            adapter.current.CreateDate = DateTime.Now.Ticks;
                                            adapter.current.DocHeadF = cs.BaseClass.currentDocHead.DocHeadF;
                                            adapter.current.GoodF = "999999999999999999999999999999";
                                            adapter.current.Good = dataBase.dataContext.Good/*.AsNoTracking()*/
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                .Where(c => c.GoodF == adapter.current.GoodF).First_Ext();
                                            ////////adapter.current.Have_comment = false;
                                            ////////adapter.current.Have_spec_comment = true;
                                            adapter.current.Spec_comment = ConnectString;
                                            //ручной ввод
                                            adapter.current.Hend_enter = cs.BaseClass.HendEnter;
                                            adapter.current.DocDetailsF = Guid.NewGuid();
                                            //////adapter.current.Price = 0;

                                            ////////adapter.current.SummDoc = null;
                                            ////////adapter.current.SummReal = null;
                                            adapter.current.UpdatedFromTSD = true;
                                            adapter.current.UpdateFrom1C = false;
                                            adapter.current.UserF = cs.BaseClass.currentIdUser;
                                            if (SmartStoreData.SourceDataBase.LocalDB == SmartStoreData.TypeDatabaseEnum.local)
                                            {
                                                adapter.current.id = 1;
                                                try
                                                {
                                                    LinqExtensions.WriteErrorToFile = false;
                                                    //29.08.2025 заменил .Max на .Max_Ext
                                                    adapter.current.id = dataBase.dataContext.DocDetails.AsNoTracking().Max_Ext(c => c.id);
                                                    LinqExtensions.WriteErrorToFile = true;
                                                }
                                                catch
                                                {
                                                }
                                            }

                                            dataBase.dataContext.DocDetails.Add(adapter.current);
                                        }

                                        try
                                        {
                                            int y = dataBase.dataContext.SaveChanges();

                                            if (y != 1)
                                            {
                                                if (list.Count > 0)
                                                {
                                                    this.RunOnUiThread(() =>
                                                    {
                                                        cs.ScanerInit.ActiveScaner(false);
                                                        cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                            .SetCancelable(false)
                                                            .SetMessage(
                                                                /*"Не удалось обновить информацию о товаре со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message35) + " " +
                                                                barcode + " " +/*"в текущем документе"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message32))
                                                            .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                            .Show();
                                                    });
                                                    return;
                                                }
                                                else
                                                {
                                                    this.RunOnUiThread(() =>
                                                    {
                                                        cs.ScanerInit.ActiveScaner(false);
                                                        cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                            .SetMessage(
                                                                /*"Не удалось добавить товар со штрихкодом"*/ Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message40) + " " + barcode +
                                                                " " +/*"в текущий документ"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message41))
                                                            .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                            .SetCancelable(false)
                                                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                            .Show();
                                                    });
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                if (!Finded)
                                                {
                                                    adapter.Insert(adapter.current, 0);
                                                }

                                                if (list.Count > 0)
                                                {
                                                    using (cs.ScanHistory history = new cs.ScanHistory())
                                                    {
                                                        //01.09.2025 заменил имя метода First на First_Ext
                                                        var tre = /*await*/ history.AddHistory/*Async*/(this, list.First_Ext(), 1, barcode, 0);
                                                        if (tre == false)
                                                        {
                                                            this.RunOnUiThread(() =>
                                                            {
                                                                cs.ScanerInit.ActiveScaner(false);
                                                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                    .SetMessage(
                                                                        /*"Не удалось обновить историю сканирований товара со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message37) + " " +
                                                                        barcode + " " +/*"для текущего документа"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message36))
                                                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                                    .SetCancelable(false)
                                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                        (senderAlert, args) =>
                                                                        {
                                                                            cs.ScanerInit.ActiveScaner(true);
                                                                        })
                                                                    .Show();
                                                            });
                                                        }
                                                        else
                                                        {
                                                            adapter.current.ScanHistory.Add(cs.ScanHistory.history);
                                                            adapter.current.Count_Real += 1;
                                                            adapter.Update();
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    using (cs.ScanHistory history = new cs.ScanHistory())
                                                    {
                                                        var tre = /*await*/ history.AddHistory/*Async*/(this, adapter.current, 1, barcode, 0);

                                                        if (tre == false)
                                                        {
                                                            this.RunOnUiThread(() =>
                                                            {
                                                                cs.ScanerInit.ActiveScaner(false);
                                                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");

                                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                    .SetMessage(
                                                                        /*"Не удалось обновить историю сканирований товара со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message37) + " " +
                                                                        barcode + " " +/*"для текущего документа"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message36))
                                                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                                    .SetCancelable(false)
                                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                        (senderAlert, args) =>
                                                                        {
                                                                            cs.ScanerInit.ActiveScaner(true);
                                                                        })
                                                                    .Show();
                                                            });
                                                        }
                                                        else
                                                        {
                                                            adapter.current.ScanHistory.Add(cs.ScanHistory.history);
                                                            adapter.current.Count_Real += 1;
                                                            adapter.Update();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            this.RunOnUiThread(() =>
                                            {
                                                cs.ScanerInit.ActiveScaner(false);
                                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");

                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                    .SetMessage(
                                                        /*"Не удалось обновить/добавить информацию о товаре со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message44) + " " +
                                                        barcode + " " +/*"в текущем документе"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message32))
                                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                    .SetCancelable(false)
                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                        (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                    .Show();
                                            });
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        cs.AlerrtDialogs.MessageNotAvailableContextDataBase.ShowDialog(this, "$20");
                                        return;
                                    }
                                }
                            }
                            catch (System.Data.SqlClient.SqlException ex)
                            {
                                cs.AlerrtDialogs.MessageNotAvailableDataBaseShow alertDialogNotAvailable =
                                    new cs.AlerrtDialogs.MessageNotAvailableDataBaseShow(this,
                                        ex.GetBaseException().Message);
                                return;
                            }
                            finally
                            {
                                cs.BaseClass.GC();
                            }

                            break;

                            #endregion
                    }
                }
                else
                {
                    if ((cs.BaseClass.currentDocHead.DocStatus < 2) || (cs.BaseClass.currentDocHead.DocStatus > 3))
                    {
                        cs.Toasts.aToast.ShowToast(this, /*"сканер выключен"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message46), ToastLength.Short, true);

                    }
                    else
                    {
                        cs.Toasts.aToast.ShowToast(this, /*"Изменение данных запрещено"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message43), ToastLength.Short, true);
                    }
                }
            }
            catch (System.Exception ex)
            {
                using (GenerateLogException generateLog = new GenerateLogException())
                {
                    generateLog.InfoException(ex, "ListDocDetailsActivity.ScanerInit_BarcodeDataEvent#1", cs.BaseClass.FreeMemoryMessage());
                }
            }
            finally
            {

                //cs.ScanerInit.StatusScanner = true;
            }
        }

        private void CollectionOfBarcodesWithCharacteristicsDialog_DismissEvent(
            cs.DialogFragments.MYDialogFragment dialogFragment)
        {
            cs.DocDetails.CollectionOfBarcodesWithCharacteristicsDialog.barcode = null;
        }

        #endregion

        #region обрабатывает полученные данные по штрихкоду

        /// <summary>
        /// обрабатывает полученные данные по штрихкоду
        /// </summary>
        /// <param name="dataBase"></param>
        /// <param name="goods"></param>
        /// <param name="barcodes"></param>
        /// <param name="barcode"></param>
        private /*async*/ void barcodeScanOperation(SmartStoreData.SourceDataBase dataBase, List<SmartStoreData.Good> goods,
            List<SmartStoreData.Barcode> barcodes, string barcode, cs.StatusBarcodeModel statusBarcodeModel)
        {
            try
            {

                ////добавил блок 19.09.2025_#4 . Важно при работе с партионным учетом
                //if (NotReadStatusBarcode)
                //{
                //    NotReadStatusBarcode = false;
                //}
                ////КОНЕЦ добавил блок 19.09.2025_#4 . Важно при работе с партионным учетом

                #region если со штрихкодом не связано ни одного товара

                if (goods.Count == 0)
                {

                    //string messageAlertD = string.Format("Ни одного товара, соответствующего штрихкоду {0} не было найдено", CurrentBarcode);
                    string messageAlertD = string.Format(Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message47), CurrentBarcode);
                    if (statusBarcodeModel.StatusProduct)
                    {
                        //messageAlertD = "Ни одного товара с кодом " + statusBarcodeModel.CodeProduct + " не было найдено";
                        messageAlertD = string.Format(Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message48), statusBarcodeModel.CodeProduct);
                    }

                    this.RunOnUiThread(() =>
                    {
                        cs.ScanerInit.ActiveScaner(false);
                        cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                            .SetMessage(messageAlertD)
                            .SetTitle(/*"Штрихкод"*/Android.App.Application.Context.GetText(Resource.String.Layout_CardProductLandscape_BarcodeFull))
                            .SetCancelable(false)
                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK), (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                            .Show();
                    });
                    return;
                }

                #endregion

                #region если найден связанный со штрихкодом товар(ы)

                else
                {
                    #region если найден один товар

                    if (goods.Count > 0)
                    {
                        if (goods.Count > 1)
                        {
                            //foreach (var item in goods)
                            //{
                            //    if (item.GoodF == barcodes[0].GoodF)
                            //    {
                            //        using (SmartStoreData.SourceDataBase dataBase2 =
                            //            new SmartStoreData.SourceDataBase(cs.BaseClass.pathDocs))
                            //        {
                            //            try
                            //            {
                            //                barcodeScanOperation(dataBase2, new List<SmartStoreData.Good>() { item },
                            //                    barcodes, CurrentBarcode, statusBarcodeModel);
                            //            }
                            //            catch (System.Data.SqlClient.SqlException ex)
                            //            {
                            //                cs.AlerrtDialogs.MessageNotAvailableDataBaseShow alertDialogNotAvailable =
                            //                    new cs.AlerrtDialogs.MessageNotAvailableDataBaseShow(this,
                            //                        ex.GetBaseException().Message);
                            //                return;
                            //            }
                            //        }

                            //        break;
                            //    }
                            //}
                            //01.09.2025 заменил имя метода Select на Select_Ext
                            string[] vremgoods = goods.Select_Ext(c => c.Name).Distinct().ToArray();
                            var activityvrem = this;
                            cs.ScanerInit.ActiveScaner(false);
                            var alertDialog = new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                .SetItems(vremgoods, (sender2, args) =>
                                {
                                    using (SmartStoreData.SourceDataBase dataBase2 =
                                        new SmartStoreData.SourceDataBase(/*cs.BaseClass.pathDocs*/))
                                    {
                                        dataBase.SourceDataBaseMethod(cs.BaseClass.pathDocs);
                                        if (dataBase2.ContextBDExsists)
                                        {
                                            dataBase2.dataContext.generateLogExceptionProcess = this;
                                            try
                                            {
                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                goods = goods.Where(c => c.Name == vremgoods[args.Which]).ToList_Ext();
                                                barcodeScanOperation(dataBase2, goods, barcodes, CurrentBarcode,
                                                    statusBarcodeModel);
                                            }
                                            catch (System.Data.SqlClient.SqlException ex)
                                            {
                                                cs.AlerrtDialogs.MessageNotAvailableDataBaseShow alertDialogNotAvailable =
                                                    new cs.AlerrtDialogs.MessageNotAvailableDataBaseShow(this,
                                                        ex.GetBaseException().Message);
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            cs.AlerrtDialogs.MessageNotAvailableContextDataBase.ShowDialog(this, "$20");
                                            return;
                                        }
                                    }
                                })
                                .SetCancelable(false)
                                .SetTitle(/*"Выберите товар из списка"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_FindBarcodeOrCodeProductDialog_Message8))
                                .Show();
                            cs.Sound.NotificationSound.PlayNotification(this, "quetion");
                            alertDialog.DismissEvent += (sender5, e5) => { cs.ScanerInit.ActiveScaner(true); };
                            return;
                        }

                        posIndex = -1;

                        //был ли найден элемент в списке
                        bool Finded = false;

                        //добавил 11.12.2024_#3
                        //был ли найден товар в списке (без учета ячейки)
                        bool FindedProduct = false;

                        #region нахожу, если таковой имеется, элемент в списке
                        if (cs.DocDetails.ListDocDetailsArray./*mOriginalValues*/ListData != null)
                        {
                            if (cs.DocDetails.ListDocDetailsArray./*mOriginalValues*/ListData.Count > 0)
                            {
                                //17.04.2026 заменил mOriginalValues на ListData
                                for (int i = 0; i < cs.DocDetails.ListDocDetailsArray./*mOriginalValues*/ListData.Count /* adapter.Count*/; i++)
                                {
                                    //17.04.2026 заменил mOriginalValues на ListData
                                    SmartStoreData.DocDetails
                                        docDetails = cs.DocDetails.ListDocDetailsArray./*mOriginalValues*/ListData[i] /* adapter.GetItem(i)*/;
                                    foreach (var itemgood in goods)
                                    {
                                        if (!SmartStoreData.SourceDataBase.showDocDetails)
                                        {
                                            if (itemgood.GoodF == docDetails.GoodF)
                                            {
                                                //добавил 11.12.2024_#3
                                                FindedProduct = true;

                                                if (cs.BaseClass.preferences.GetBoolean("StrongCellCheckBox", true) &&
                                                    cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                                                {
                                                    if (!SmartStoreData.SourceDataBase.showDocDetails)
                                                    {
                                                        //добавил блок 15.09.2025
                                                        //02.10.2025_#2 заменил  UsedAccountingParties на UsedAccountingParties_NEW
                                                        if (cs.Settings.AccountingPartiesClass.UsedAccountingParties_NEW() &&
                                                            cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                        {
                                                            //if (docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                            //    docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                            //{
                                                            //17.04.2026 добавил блок
                                                            if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood))
                                                            {
                                                                //КОНЕЦ 17.04.2026 добавил блок
                                                                //добавил 03.09.2023
                                                                adapter.current = docDetails;
                                                                posIndex = i;
                                                                Finded = true;
                                                                break;
                                                                //17.04.2026 добавил блок
                                                            }
                                                            //КОНЕЦ 17.04.2026 добавил блок
                                                            //}
                                                        }
                                                        else
                                                        {
                                                            //КОНЕЦ добавил блок 15.09.2025
                                                            //17.04.2026 добавил блок
                                                            if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood))
                                                            {
                                                                //КОНЕЦ 17.04.2026 добавил блок
                                                                //добавил 03.09.2023
                                                                adapter.current = docDetails;
                                                                posIndex = i;
                                                                Finded = true;
                                                                break;
                                                                //17.04.2026 добавил блок
                                                            }
                                                            //КОНЕЦ 17.04.2026 добавил блок
                                                            //добавил блок 15.09.2025
                                                        }
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                    }
                                                    else
                                                    {

                                                        //добавил 07.02.2024
                                                        if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                        {
                                                            //добавил блок 15.09.2025
                                                            //02.10.2025_#2 закоментировал блок
                                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() &&
                                                            //    cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                            //{
                                                            //    if (docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                            //        docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                            //    {
                                                            //        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                                            //        adapter.current = docDetails;
                                                            //        posIndex = i;
                                                            //        Finded = true;
                                                            //        break;
                                                            //    }
                                                            //}
                                                            //else
                                                            //{
                                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                            //КОНЕЦ добавил блок 15.09.2025
                                                            //17.04.2026 добавил блок
                                                            if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood))
                                                            {
                                                                //КОНЕЦ 17.04.2026 добавил блок
                                                                //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                                                adapter.current = docDetails;
                                                                posIndex = i;
                                                                Finded = true;
                                                                break;
                                                                //17.04.2026 добавил блок
                                                            }
                                                            //КОНЕЦ 17.04.2026 добавил блок
                                                            //добавил блок 15.09.2025
                                                            //02.10.2025_#2 закоментировал блок
                                                            //}
                                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                            //КОНЕЦ добавил блок 15.09.2025
                                                        }
                                                        else
                                                        {
                                                            //добавил блок 19.06.2025
                                                            if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                                            {
                                                                //добавил блок 15.09.2025
                                                                //02.10.2025_#2 закоментировал блок
                                                                //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() &&
                                                                //    cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                                //{
                                                                //    if (docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                                //        docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                                //    {
                                                                //        adapter.current = docDetails;
                                                                //        posIndex = i;
                                                                //        Finded = true;
                                                                //        break;
                                                                //    }
                                                                //}
                                                                //else
                                                                //{
                                                                //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                //КОНЕЦ добавил блок 15.09.2025
                                                                //17.04.2026 добавил блок
                                                                if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood))
                                                                {
                                                                    //КОНЕЦ 17.04.2026 добавил блок     
                                                                    adapter.current = docDetails;
                                                                    posIndex = i;
                                                                    Finded = true;
                                                                    break;
                                                                    //17.04.2026 добавил блок
                                                                }
                                                                //КОНЕЦ 17.04.2026 добавил блок
                                                                //добавил блок 15.09.2025
                                                                //02.10.2025_#2 закоментировал блок
                                                                //}
                                                                //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                //КОНЕЦ добавил блок 15.09.2025
                                                            }
                                                            else
                                                            {
                                                                //КОНЕЦ добавил блок 19.06.2025
                                                                //ошибка (устранена 03.09.2023)
                                                                //17.04.2026 добавил блок
                                                                if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood))
                                                                {
                                                                    //КОНЕЦ 17.04.2026 добавил блок  
                                                                    foreach (var item7 in docDetails.cells)
                                                                    {
                                                                        if (item7 != null)
                                                                        {
                                                                            //добавил блок 15.09.2025
                                                                            //02.10.2025_#2 закоментировал блок
                                                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() &&
                                                                            //    cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                                            //{
                                                                            //    if (item7.CellF == cs.BaseClass.currentIdCell &&
                                                                            //        docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                                            //        docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                                            //    {
                                                                            //        adapter.current = docDetails;
                                                                            //        posIndex = i;
                                                                            //        Finded = true;
                                                                            //        break;
                                                                            //    }
                                                                            //}
                                                                            //else
                                                                            //{
                                                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                            //КОНЕЦ добавил блок 15.09.2025
                                                                            if (item7.CellF == cs.BaseClass.currentIdCell)
                                                                            {
                                                                                adapter.current = docDetails;
                                                                                posIndex = i;
                                                                                Finded = true;
                                                                                break;
                                                                            }
                                                                            //добавил блок 15.09.2025
                                                                            //02.10.2025_#2 закоментировал блок
                                                                            //}
                                                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                            //КОНЕЦ добавил блок 15.09.2025
                                                                        }
                                                                    }
                                                                    //17.04.2026 добавил блок
                                                                }
                                                                //КОНЕЦ 17.04.2026 добавил блок  
                                                                if (!Finded)
                                                                {
                                                                    //17.04.2026 добавил блок
                                                                    if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood))
                                                                    {
                                                                        //КОНЕЦ 17.04.2026 добавил блок  
                                                                        foreach (var item7 in docDetails.cells)
                                                                        {
                                                                            //добавил блок 15.09.2025
                                                                            //02.10.2025_#2 закоментировал блок
                                                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() &&
                                                                            //    cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                                            //{
                                                                            //    if (item7 == null &&
                                                                            //        docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                                            //        docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                                            //    {
                                                                            //        adapter.current = docDetails;
                                                                            //        posIndex = i;
                                                                            //        Finded = true;
                                                                            //        break;
                                                                            //    }
                                                                            //}
                                                                            //else
                                                                            //{
                                                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                            //КОНЕЦ добавил блок 15.09.2025
                                                                            if (item7 == null)
                                                                            {
                                                                                adapter.current = docDetails;
                                                                                posIndex = i;
                                                                                Finded = true;
                                                                                break;
                                                                            }
                                                                            //добавил блок 15.09.2025
                                                                            //02.10.2025_#2 закоментировал блок
                                                                            //}
                                                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                            //КОНЕЦ добавил блок 15.09.2025
                                                                        }
                                                                        //17.04.2026 добавил блок
                                                                    }
                                                                    //КОНЕЦ 17.04.2026 добавил блок
                                                                }
                                                                else
                                                                {
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //добавил блок 15.09.2025
                                                    //02.10.2025_#2 закоментировал блок
                                                    //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() &&
                                                    //    cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                    //{
                                                    //    if (docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                    //        docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                    //    {
                                                    //        adapter.current = docDetails;
                                                    //        posIndex = i;
                                                    //        Finded = true;
                                                    //        break;
                                                    //    }
                                                    //}
                                                    //else
                                                    //{
                                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                    //КОНЕЦ добавил блок 15.09.2025

                                                    //17.04.2026 добавил блок
                                                    if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, goods[0]))
                                                    {
                                                        //КОНЕЦ 17.04.2026 добавил блок
                                                        adapter.current = docDetails;
                                                        posIndex = i;
                                                        Finded = true;
                                                        break;
                                                        //17.04.2026 добавил блок
                                                    }
                                                    //КОНЕЦ 17.04.2026 добавил блок
                                                    //добавил блок 15.09.2025
                                                    //02.10.2025_#2 закоментировал блок
                                                    //}
                                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                    //КОНЕЦ добавил блок 15.09.2025
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //если строгий режим ячеек
                                            if (cs.BaseClass.preferences.GetBoolean("StrongCellCheckBox", true) &&
                                                cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                                            {
                                                if (itemgood.GoodF == docDetails.GoodF)
                                                {
                                                    //добавил 11.12.2024_#3
                                                    FindedProduct = true;

                                                    //добавил 07.02.2024
                                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                    {
                                                        //добавил блок 15.09.2025
                                                        //02.10.2025_#2 закоментировал блок
                                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() &&
                                                        //    cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                        //{
                                                        //    if (docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                        //        docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                        //    {
                                                        //        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                                        //        adapter.current = docDetails;
                                                        //        posIndex = i;
                                                        //        Finded = true;
                                                        //        break;
                                                        //    }
                                                        //}
                                                        //else
                                                        //{
                                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                        //17.04.2026 добавил блок
                                                        if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood))
                                                        {
                                                            //КОНЕЦ 17.04.2026 добавил блок  
                                                            //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                                            adapter.current = docDetails;
                                                            posIndex = i;
                                                            Finded = true;
                                                            break;
                                                            //17.04.2026 добавил блок
                                                        }
                                                        //КОНЕЦ 17.04.2026 добавил блок 
                                                        //добавил блок 15.09.2025
                                                        //02.10.2025_#2 закоментировал блок
                                                        //}
                                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                    }
                                                    else
                                                    {
                                                        //добавил блок 19.06.2025
                                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                                        {
                                                            //добавил блок 15.09.2025
                                                            //02.10.2025_#2 закоментировал блок
                                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() &&
                                                            //    cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                            //{
                                                            //    if (docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                            //        docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                            //    {
                                                            //        adapter.current = docDetails;
                                                            //        posIndex = i;
                                                            //        Finded = true;
                                                            //        break;
                                                            //    }
                                                            //}
                                                            //else
                                                            //{
                                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                            //КОНЕЦ добавил блок 15.09.2025
                                                            //17.04.2026 добавил блок
                                                            if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood))
                                                            {
                                                                //КОНЕЦ 17.04.2026 добавил блок  
                                                                adapter.current = docDetails;
                                                                posIndex = i;
                                                                Finded = true;
                                                                break;
                                                                //17.04.2026 добавил блок
                                                            }
                                                            //КОНЕЦ 17.04.2026 добавил блок  
                                                            //добавил блок 15.09.2025
                                                            //02.10.2025_#2 закоментировал блок
                                                            //}
                                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                            //КОНЕЦ добавил блок 15.09.2025
                                                        }
                                                        else
                                                        {
                                                            //КОНЕЦ добавил блок 19.06.2025
                                                            //добавил блок 15.09.2025
                                                            //02.10.2025_#2 закоментировал блок
                                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                            //{
                                                            //    if (docDetails.CellF == cs.BaseClass.currentIdCell &&
                                                            //        docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                            //        docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                            //    {
                                                            //        adapter.current = docDetails;
                                                            //        posIndex = i;
                                                            //        Finded = true;
                                                            //        break;
                                                            //    }
                                                            //}
                                                            //else
                                                            //{
                                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                            //КОНЕЦ добавил блок 15.09.2025
                                                            //17.04.2026 добавил блок
                                                            if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood, cs.BaseClass.currentIdCell))
                                                            {
                                                                //КОНЕЦ 17.04.2026 добавил блок 
                                                                if (docDetails.CellF == cs.BaseClass.currentIdCell)
                                                                {
                                                                    adapter.current = docDetails;
                                                                    posIndex = i;
                                                                    Finded = true;
                                                                    break;
                                                                }
                                                                //17.04.2026 добавил блок
                                                            }
                                                            //КОНЕЦ 17.04.2026 добавил блок
                                                            //добавил блок 15.09.2025
                                                            //02.10.2025_#2 закоментировал блок
                                                            //}
                                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                            //КОНЕЦ добавил блок 15.09.2025

                                                            if (!Finded)
                                                            {
                                                                //17.04.2026 добавил блок
                                                                if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood))
                                                                {
                                                                    //КОНЕЦ 17.04.2026 добавил блок 
                                                                    foreach (var item7 in docDetails.cells)
                                                                    {
                                                                        //добавил блок 15.09.2025
                                                                        //02.10.2025_#2 закоментировал блок
                                                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                                        //{
                                                                        //    if (item7 == null &&
                                                                        //        docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                                        //        docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                                        //    {
                                                                        //        adapter.current = docDetails;
                                                                        //        posIndex = i;
                                                                        //        Finded = true;
                                                                        //        break;
                                                                        //    }
                                                                        //}
                                                                        //else
                                                                        //{
                                                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                        //КОНЕЦ добавил блок 15.09.2025
                                                                        if (item7 == null)
                                                                        {
                                                                            adapter.current = docDetails;
                                                                            posIndex = i;
                                                                            Finded = true;
                                                                            break;
                                                                        }
                                                                        //добавил блок 15.09.2025
                                                                        //02.10.2025_#2 закоментировал блок
                                                                        //}
                                                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                        //КОНЕЦ добавил блок 15.09.2025
                                                                    }
                                                                    //17.04.2026 добавил блок
                                                                }
                                                                //КОНЕЦ 17.04.2026 добавил блок 
                                                            }
                                                            else
                                                            {
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //добавил блок 25.01.2026_#4
                                                //if (Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.IsWaveAssemblyDocument(true))
                                                //{
                                                //    //добавил 11.12.2024_#3
                                                //    FindedProduct = true;
                                                //    adapter.current = docDetails;
                                                //    posIndex = i;
                                                //    Finded = true;
                                                //    break;
                                                //}
                                                //else
                                                //{
                                                //КОНЕЦ добавил блок 25.01.2026_#4
                                                //добавил 07.02.2024
                                                if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                {
                                                    //17.04.2026 добавил блок
                                                    if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood, null))
                                                    {
                                                        //КОНЕЦ 17.04.2026 добавил блок 
                                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                                        if (itemgood.GoodF == docDetails.GoodF && docDetails.CellF == null)
                                                        {
                                                            //добавил 11.12.2024_#3
                                                            FindedProduct = true;

                                                            //добавил блок 15.09.2025
                                                            //02.10.2025_#2 закоментировал блок
                                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                            //{
                                                            //    if (docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                            //        docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                            //    {
                                                            //        adapter.current = docDetails;
                                                            //        posIndex = i;
                                                            //        Finded = true;
                                                            //        break;
                                                            //    }
                                                            //}
                                                            //else
                                                            //{
                                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                            //КОНЕЦ добавил блок 15.09.2025
                                                            adapter.current = docDetails;
                                                            posIndex = i;
                                                            Finded = true;
                                                            break;
                                                            //добавил блок 15.09.2025
                                                            //02.10.2025_#2 закоментировал блок
                                                            //}
                                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                            //КОНЕЦ добавил блок 15.09.2025
                                                        }
                                                        //17.04.2026 добавил блок
                                                    }
                                                    //КОНЕЦ 17.04.2026 добавил блок 
                                                }
                                                else
                                                {
                                                    //добавил блок 19.06.2025
                                                    if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                                    {
                                                        //17.04.2026 добавил блок
                                                        if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood, null))
                                                        {
                                                            //КОНЕЦ 17.04.2026 добавил блок 
                                                            if (itemgood.GoodF == docDetails.GoodF && docDetails.CellF == null)
                                                            {
                                                                //добавил 11.12.2024_#3
                                                                FindedProduct = true;

                                                                //добавил блок 15.09.2025
                                                                //02.10.2025_#2 закоментировал блок
                                                                //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                                //{
                                                                //    if (docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                                //        docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                                //    {
                                                                //        adapter.current = docDetails;
                                                                //        posIndex = i;
                                                                //        Finded = true;
                                                                //        break;
                                                                //    }
                                                                //}
                                                                //else
                                                                //{
                                                                //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                //КОНЕЦ добавил блок 15.09.2025
                                                                adapter.current = docDetails;
                                                                posIndex = i;
                                                                Finded = true;
                                                                break;
                                                                //добавил блок 15.09.2025
                                                                //02.10.2025_#2 закоментировал блок
                                                                //}
                                                                //02.10.2025_#2 закоментировал блок
                                                                //КОНЕЦ добавил блок 15.09.2025
                                                            }
                                                            //17.04.2026 добавил блок
                                                        }
                                                        //КОНЕЦ 17.04.2026 добавил блок 
                                                    }
                                                    else
                                                    {
                                                        //КОНЕЦ добавил блок 19.06.2025
                                                        //17.04.2026 добавил блок
                                                        if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood, cs.BaseClass.currentIdCell))
                                                        {
                                                            //КОНЕЦ 17.04.2026 добавил блок 
                                                            if (itemgood.GoodF == docDetails.GoodF &&
                                                    docDetails.CellF == cs.BaseClass.currentIdCell)
                                                            {
                                                                //добавил 11.12.2024_#3
                                                                FindedProduct = true;

                                                                //добавил блок 15.09.2025
                                                                //02.10.2025_#2 закоментировал блок
                                                                //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                                //{
                                                                //    if (docDetails.BatchNumber==statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                                //        docDetails.ViewPack== cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                                //    {
                                                                //        FindedProduct = true;
                                                                //        adapter.current = docDetails;
                                                                //        posIndex = i;
                                                                //        Finded = true;
                                                                //        break;
                                                                //    }    
                                                                //}
                                                                //else
                                                                //{
                                                                //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                //КОНЕЦ добавил блок 15.09.2025

                                                                adapter.current = docDetails;
                                                                posIndex = i;
                                                                Finded = true;
                                                                break;
                                                                //добавил блок 15.09.2025
                                                                //02.10.2025_#2 закоментировал блок
                                                                //}
                                                                //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                //КОНЕЦ добавил блок 15.09.2025
                                                            }
                                                            //17.04.2026 добавил блок
                                                        }
                                                        //КОНЕЦ 17.04.2026 добавил блок 
                                                        if (!Finded)
                                                        {
                                                            //17.04.2026 добавил блок
                                                            if (cs.DocDetails.ListDocDetailsHelpers.ListDocDetailsHelper.Instance.GetDocDetailsIfMoreOncePosition(docDetails, itemgood, null))
                                                            {
                                                                //КОНЕЦ 17.04.2026 добавил блок 
                                                                if (itemgood.GoodF == docDetails.GoodF && docDetails.CellF == null)
                                                                {
                                                                    //добавил 11.12.2024_#3
                                                                    FindedProduct = true;

                                                                    //добавил блок 15.09.2025
                                                                    //02.10.2025_#2 закоментировал блок
                                                                    //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                                    //{
                                                                    //    if (docDetails.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                                    //        docDetails.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                                                    //    {
                                                                    //        adapter.current = docDetails;
                                                                    //        posIndex = i;
                                                                    //        Finded = true;
                                                                    //        break;
                                                                    //    }
                                                                    //}
                                                                    //else
                                                                    //{
                                                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                    //КОНЕЦ добавил блок 15.09.2025

                                                                    adapter.current = docDetails;
                                                                    posIndex = i;
                                                                    Finded = true;
                                                                    break;
                                                                    //добавил блок 15.09.2025
                                                                    //02.10.2025_#2 закоментировал блок
                                                                    //}
                                                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                                    //КОНЕЦ добавил блок 15.09.2025
                                                                }
                                                                //17.04.2026 добавил блок
                                                            }
                                                            //КОНЕЦ 17.04.2026 добавил блок 
                                                        }
                                                        else
                                                        {
                                                            break;
                                                        }
                                                    }
                                                }
                                                //добавил блок 25.01.2026_#4
                                                //}
                                                //КОНЕЦ добавил блок 25.01.2026_#4
                                            }
                                        }
                                    }
                                    if (Finded)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion

                        if (!Finded)
                        {
                            #region нахожу, если таковой имеется, в документе (из неотфильтрованного списка)
                            List<DocDetails> vremgoodsList = new List<DocDetails>();
                            //если свернутый список
                            if (!SmartStoreData.SourceDataBase.showDocDetails)
                            {
                                if (cs.BaseClass.preferences.GetBoolean("StrongCellCheckBox", true) &&
                                    cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                                {
                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                        //{
                                        //    //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        //    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                        //            .Where(c => c.GoodF == goods[0].GoodF &&
                                        //            (c.CellF == null || c.CellF == 0) &&
                                        //            c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                        //            c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                        //            .ToList_Ext();
                                        //}
                                        //else
                                        //{
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                                .Where(c => c.GoodF == goods[0].GoodF && (c.CellF == null || c.CellF == 0))
                                                .ToList_Ext();
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //}
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                    }
                                    else
                                    {
                                        //добавил блок 19.06.2025
                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                        {
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                            //        .Where(c => c.GoodF == goods[0].GoodF &&
                                            //        (c.CellF == null || c.CellF == 0) &&
                                            //        c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //        c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //        .ToList_Ext();
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                                .Where(c => c.GoodF == goods[0].GoodF && (c.CellF == null || c.CellF == 0))
                                                .ToList_Ext();
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                        else
                                        {
                                            //КОНЕЦ добавил блок 19.06.2025

                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                            //                    .Where(c => c.GoodF == goods[0].GoodF &&
                                            //                    c.CellF == cs.BaseClass.currentIdCell &&
                                            //                    c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //                    c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //                    .ToList_Ext();
                                            //    if (vremgoodsList.Count == 0)
                                            //    {
                                            //        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //        vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                            //                        .Where(c => c.GoodF == goods[0].GoodF &&
                                            //                        c.CellF == null &&
                                            //                        c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //                        c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //            .ToList_Ext();
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                                            .Where(c => c.GoodF == goods[0].GoodF &&
                                                            c.CellF == cs.BaseClass.currentIdCell)
                                                            .ToList_Ext();
                                            if (vremgoodsList.Count == 0)
                                            {
                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                                                .Where(c => c.GoodF == goods[0].GoodF &&
                                                                c.CellF == null)
                                                                .ToList_Ext();
                                            }
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                    }
                                }
                                else
                                {
                                    //добавил блок 15.09.2025
                                    //02.10.2025_#2 закоментировал блок
                                    //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                    //{
                                    //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                    //    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                    //    .Where(c => c.GoodF == goods[0].GoodF &&
                                    //    c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                    //    c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).ToList_Ext();
                                    //}
                                    //else
                                    //{
                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                    //КОНЕЦ добавил блок 15.09.2025
                                    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                    .Where(c => c.GoodF == goods[0].GoodF).ToList_Ext();
                                    //добавил блок 15.09.2025
                                    //02.10.2025_#2 закоментировал блок
                                    //}
                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                    //КОНЕЦ добавил блок 15.09.2025
                                }
                            }
                            //если развернутый список
                            else
                            {
                                //если строгий режим ячеек
                                if (cs.BaseClass.preferences.GetBoolean("StrongCellCheckBox", true) &&
                                    cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                                {
                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                        //{
                                        //    //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        //    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                        //    .Where(c => c.GoodF == goods[0].GoodF && 
                                        //           c.CellF == null &&
                                        //           c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                        //           c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                        //           .ToList_Ext();
                                        //}
                                        //else
                                        //{
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                        .Where(c => c.GoodF == goods[0].GoodF && c.CellF == null)
                                        .ToList_Ext();
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //}
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                    }
                                    else
                                    {
                                        //добавил блок 19.06.2025
                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                        {
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                            //            .Where(c => c.GoodF == goods[0].GoodF && 
                                            //            c.CellF == null &&
                                            //            c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //            c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //            .ToList_Ext();
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                        .Where(c => c.GoodF == goods[0].GoodF && c.CellF == null)
                                        .ToList_Ext();
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                        else
                                        {
                                            //КОНЕЦ добавил блок 19.06.2025
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                            //                    .Where(c => c.GoodF == goods[0].GoodF && 
                                            //                     c.CellF == cs.BaseClass.currentIdCell &&
                                            //                     c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //                     c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //                    .ToList_Ext();
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                                            .Where(c => c.GoodF == goods[0].GoodF && c.CellF == cs.BaseClass.currentIdCell)
                                                            .ToList_Ext();
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                    }
                                }
                                else
                                {
                                    //добавил блок 25.01.2026_#4
                                    //if (Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.IsWaveAssemblyDocument(true))
                                    //{
                                    //    //добавил блок 15.09.2025
                                    //    //02.10.2025_#2 закоментировал блок
                                    //    //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                    //    //{
                                    //    //    //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                    //    //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                    //    //    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                    //    //            .Where(c => c.GoodF == goods[0].GoodF && 
                                    //    //                   c.CellF == null &&
                                    //    //                   c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                    //    //                   c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                    //    //                   .ToList_Ext();
                                    //    //}
                                    //    //else
                                    //    //{
                                    //    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                    //    //КОНЕЦ добавил блок 15.09.2025
                                    //    //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                    //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                    //    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                    //            .Where(c => c.GoodF == goods[0].GoodF).ToList_Ext();
                                    //    //добавил блок 15.09.2025
                                    //    //02.10.2025_#2 закоментировал блок
                                    //    //}
                                    //    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                    //    //КОНЕЦ добавил блок 15.09.2025
                                    //}
                                    //else
                                    //{
                                    //КОНЕЦ добавил блок 25.01.2026_#4
                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                        //{
                                        //    //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        //    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                        //            .Where(c => c.GoodF == goods[0].GoodF && 
                                        //                   c.CellF == null &&
                                        //                   c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                        //                   c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                        //                   .ToList_Ext();
                                        //}
                                        //else
                                        //{
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                                .Where(c => c.GoodF == goods[0].GoodF && c.CellF == null).ToList_Ext();
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //}
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                    }
                                    else
                                    {
                                        //добавил блок 19.06.2025
                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                        {
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                            //        .Where(c => c.GoodF == goods[0].GoodF && 
                                            //               c.CellF == null &&
                                            //               c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //               c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //               .ToList_Ext();
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                                .Where(c => c.GoodF == goods[0].GoodF && c.CellF == null).ToList_Ext();
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                        else
                                        {
                                            //КОНЕЦ добавил блок 19.06.2025

                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                            //                    .Where(c => c.GoodF == goods[0].GoodF && 
                                            //                    c.CellF == cs.BaseClass.currentIdCell &&
                                            //                    c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //                    c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //                    .ToList_Ext();
                                            //    if (vremgoodsList.Count == 0)
                                            //    {
                                            //        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //        vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                            //            .Where(c => c.GoodF == goods[0].GoodF && 
                                            //            c.CellF == null &&
                                            //            c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //            c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //            .ToList_Ext();
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                    .Where(c => c.GoodF == goods[0].GoodF && c.CellF == cs.BaseClass.currentIdCell)
                                    .ToList_Ext();
                                            if (vremgoodsList.Count == 0)
                                            {
                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                vremgoodsList = cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                                    .Where(c => c.GoodF == goods[0].GoodF && c.CellF == null).ToList_Ext();
                                            }
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                    }
                                    //добавил блок 25.01.2026_#4
                                    //}
                                    //КОНЕЦ добавил блок 25.01.2026_#4
                                }
                            }

                            if (vremgoodsList.Count > 0)
                            {
                                //добавил блок 17.04.2026
                                if (vremgoodsList.Count > 1)
                                {
                                    //Найдено несколько позиций в документе, соответствующих отсканированному штрихкоду.
                                    //В приоритете будет выбрана позиция где CountReal=0,
                                    //а если нет таких, следующие будут те, в которых план > факта,
                                    //а потом уже те котрые собраны полностью, при условии, что такие существуют.
                                    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                    vremgoodsList = vremgoodsList
                                        //.OrderBy(c => c.Count_Real != 0)
                                        //06.05.2026 добавил SmartSortByCountReal()
                                        .SmartSortByCountReal()
                                        .ToList_Ext();

                                }
                                //КОНЕЦ добавил блок 17.04.2026
                                posIndex = adapter.GetPosition(vremgoodsList[0]);
                                adapter.current = vremgoodsList[0];
                                Finded = true;
                                //добавил 11.12.2024_#3
                                FindedProduct = true;
                            }
                            else
                            {
                                //добавил блок 11.12.2024_#3
                                if (!FindedProduct)
                                {
                                    //добавил блок 15.09.2025
                                    //02.10.2025_#2 закоментировал блок
                                    //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                    //{
                                    //    //01.09.2025 заменил имя метода Count на Count_Ext
                                    //    if (cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                    //                        .Where(c => c.GoodF == goods[0].GoodF &&
                                    //                        c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                    //                        c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                    //                        .Count_Ext() > 0)
                                    //    {
                                    //        FindedProduct = true;
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                    //КОНЕЦ добавил блок 15.09.2025
                                    //01.09.2025 заменил имя метода Count на Count_Ext
                                    if (cs.DocDetails.ListDocDetailsArray.mOriginalValues
                                                        .Where(c => c.GoodF == goods[0].GoodF)
                                                        .Count_Ext() > 0)
                                    {
                                        FindedProduct = true;
                                    }
                                    //добавил блок 15.09.2025
                                    //02.10.2025_#2 закоментировал блок
                                    //}
                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                    //КОНЕЦ добавил блок 15.09.2025
                                }
                                //КОНЕЦ добавил блок 11.12.2024_#3
                            }

                            #endregion
                        }

                        if (!Finded)
                        {
                            #region нахожу, если таковой имеется, в документе (из БД используется для актуализации данных при совместной работе с документами)

                            List<DocDetails> vremgoodsList = new List<DocDetails>();
                            //если свернутый список
                            if (!SmartStoreData.SourceDataBase.showDocDetails)
                            {
                                if (cs.BaseClass.preferences.GetBoolean("StrongCellCheckBox", true) &&
                                    cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                                {

                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                        //{
                                        //    //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        //    vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                        //.Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                        //.Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                        //    c.GoodF == goods[0].GoodF && c.CellF == null &&
                                        //    c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                        //    c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).ToList_Ext();
                                        //}
                                        //else
                                        //{
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                        c.GoodF == goods[0].GoodF && c.CellF == null).ToList_Ext();
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //}
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                    }
                                    else
                                    {
                                        //добавил блок 19.06.2025

                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                        {
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //    if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //    {
                                            //        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //        vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                            //.Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                            //.Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                            //    c.GoodF == goods[0].GoodF && c.CellF == null &&
                                            //    c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //    c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).ToList_Ext();
                                            //    }
                                            //    else
                                            //    {
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                        c.GoodF == goods[0].GoodF && c.CellF == null).ToList_Ext();
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                        else
                                        {
                                            //КОНЕЦ добавил блок 19.06.2025
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //        if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //        {
                                            //            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //            vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                            //    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                            //    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                            //        c.GoodF == goods[0].GoodF /*&& c.CellF == cs.BaseClass.currentIdCell*/&&
                                            //        c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //        c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).ToList_Ext();

                                            //            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //            var tekvremgoodsList = vremgoodsList/*dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                            //.Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                            //.Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)*/.Where(c =>
                                            //    /*c.GoodF == goods[0].GoodF &&*/ c.CellF == cs.BaseClass.currentIdCell).ToList_Ext();
                                            //            if (tekvremgoodsList.Count == 0)
                                            //            {
                                            //                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //                vremgoodsList = vremgoodsList/*dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                            //    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                            //    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)*/.Where(c =>
                                            //        /*c.GoodF == goods[0].GoodF &&*/ (c.CellF == null || c.CellF == 0)).ToList_Ext();
                                            //            }
                                            //            else
                                            //            {
                                            //                vremgoodsList = tekvremgoodsList;
                                            //            }
                                            //        }
                                            //        else
                                            //        {
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                        c.GoodF == goods[0].GoodF /*&& c.CellF == cs.BaseClass.currentIdCell*/).ToList_Ext();

                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            var tekvremgoodsList = vremgoodsList/*dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)*/.Where(c =>
                                        /*c.GoodF == goods[0].GoodF &&*/ c.CellF == cs.BaseClass.currentIdCell).ToList_Ext();
                                            if (tekvremgoodsList.Count == 0)
                                            {
                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                vremgoodsList = vremgoodsList/*dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                        .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                        .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)*/.Where(c =>
                                            /*c.GoodF == goods[0].GoodF &&*/ (c.CellF == null || c.CellF == 0)).ToList_Ext();
                                            }
                                            else
                                            {
                                                vremgoodsList = tekvremgoodsList;
                                            }
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                    }
                                }
                                else
                                {
                                    //добавил блок 15.09.2025
                                    //02.10.2025_#2 закоментировал блок
                                    //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                    //{
                                    //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                    //    vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    //    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    //    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)
                                    //    .Where(c => c.GoodF == goods[0].GoodF &&
                                    //        c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                    //        c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).ToList_Ext();
                                    //}
                                    //else
                                    //{
                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                    //КОНЕЦ добавил блок 15.09.2025
                                    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                    vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)
                                    .Where(c => c.GoodF == goods[0].GoodF).ToList_Ext();
                                    //добавил блок 15.09.2025
                                    //02.10.2025_#2 закоментировал блок
                                    //}
                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                    //КОНЕЦ добавил блок 15.09.2025
                                }
                            }
                            //если развернутый список
                            else
                            {
                                //если строгий режим ячеек
                                if (cs.BaseClass.preferences.GetBoolean("StrongCellCheckBox", true) &&
                                    cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                                {
                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                        //{
                                        //    //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        //    vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                        //     .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                        //     .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                        //         c.GoodF == goods[0].GoodF && c.CellF == null &&
                                        //    c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                        //    c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                        //     .ToList_Ext();
                                        //}
                                        //else
                                        //{
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                         .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                         .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF &&
                                         c.GoodF == goods[0].GoodF && c.CellF == null).ToList_Ext();
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //}
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                    }
                                    else
                                    {
                                        //добавил блок 19.06.2025
                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                        {
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                            // .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                            // .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                            //     c.GoodF == goods[0].GoodF && c.CellF == null &&
                                            //c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            // .ToList_Ext();
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                         .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                         .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == goods[0].GoodF && c.CellF == null).ToList_Ext();
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                        else
                                        {
                                            //КОНЕЦ добавил блок 19.06.2025
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //    if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //    {
                                            //        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //        vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                            //.Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                            //.Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                            //    c.GoodF == goods[0].GoodF && c.CellF == cs.BaseClass.currentIdCell &&
                                            //    c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //    c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //.ToList_Ext();
                                            //    }
                                            //    else
                                            //    {
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF &&
                                     c.GoodF == goods[0].GoodF && c.CellF == cs.BaseClass.currentIdCell)
                                    .ToList_Ext();
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                    }
                                }
                                else
                                {
                                    //добавил блок 25.01.2026_#4
                                    //if (Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.IsWaveAssemblyDocument(true))
                                    //{
                                    //    //добавил блок 15.09.2025
                                    //    //02.10.2025_#2 закоментировал блок
                                    //    //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                    //    //{
                                    //    //    //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                    //    //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                    //    //    vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    //    //    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    //    //    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                    //    //        c.GoodF == goods[0].GoodF && c.CellF == null &&
                                    //    //        c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                    //    //    c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                    //    //    .ToList_Ext();
                                    //    //}
                                    //    //else
                                    //    //{
                                    //    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                    //    //КОНЕЦ добавил блок 15.09.2025
                                    //    //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                    //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                    //    vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    //    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    //    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF &&
                                    //        c.GoodF == goods[0].GoodF)
                                    //    .ToList_Ext();
                                    //    //добавил блок 15.09.2025
                                    //    //02.10.2025_#2 закоментировал блок
                                    //    //}
                                    //    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                    //    //КОНЕЦ добавил блок 15.09.2025
                                    //}
                                    //else
                                    //{
                                    //КОНЕЦ добавил блок 25.01.2026_#4
                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                        //{
                                        //    //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        //    vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                        //    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                        //    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                        //        c.GoodF == goods[0].GoodF && c.CellF == null &&
                                        //        c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                        //    c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                        //    .ToList_Ext();
                                        //}
                                        //else
                                        //{
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                        .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                        .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF &&
                                            c.GoodF == goods[0].GoodF && c.CellF == null)
                                        .ToList_Ext();
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //}
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                    }
                                    else
                                    {
                                        //добавил блок 19.06.2025
                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                        {
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                            //.Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                            //.Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                            //    c.GoodF == goods[0].GoodF && c.CellF == null &&
                                            //    c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //    c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //.ToList_Ext();
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                        .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                        .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF &&
                                            c.GoodF == goods[0].GoodF && c.CellF == null)
                                        .ToList_Ext();
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                        else
                                        {
                                            //КОНЕЦ добавил блок 19.06.2025
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //        if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //        {
                                            //            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //            vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                            //    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                            //    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF).Where(c =>
                                            //        c.GoodF == goods[0].GoodF /*&& c.CellF == cs.BaseClass.currentIdCell*/&&
                                            //            c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //            c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //    .ToList_Ext();

                                            //            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //            var tekvremgoodsList = vremgoodsList/*dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                            //.Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                            //.Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)*/.Where(c =>
                                            //    /*c.GoodF == goods[0].GoodF &&*/ c.CellF == cs.BaseClass.currentIdCell)
                                            //                 .ToList_Ext();
                                            //            if (tekvremgoodsList.Count == 0)
                                            //            {
                                            //                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //                vremgoodsList = vremgoodsList/*dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                            //    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                            //    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)*/
                                            //                    .Where(c => /*c.GoodF == goods[0].GoodF &&*/ c.CellF == null).ToList_Ext();
                                            //            }
                                            //            else
                                            //            {
                                            //                vremgoodsList = tekvremgoodsList;
                                            //            }
                                            //        }
                                            //        else
                                            //        {
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            vremgoodsList = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF &&
                                        c.GoodF == goods[0].GoodF /*&& c.CellF == cs.BaseClass.currentIdCell*/)
                                    .ToList_Ext();

                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            var tekvremgoodsList = vremgoodsList/*dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)*/.Where(c =>
                                        /*c.GoodF == goods[0].GoodF &&*/ c.CellF == cs.BaseClass.currentIdCell)
                                                 .ToList_Ext();
                                            if (tekvremgoodsList.Count == 0)
                                            {
                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                vremgoodsList = vremgoodsList/*dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                        .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                        .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)*/
                                                    .Where(c => /*c.GoodF == goods[0].GoodF &&*/ c.CellF == null).ToList_Ext();
                                            }
                                            else
                                            {
                                                vremgoodsList = tekvremgoodsList;
                                            }
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                    }
                                    //добавил блок 25.01.2026_#4
                                    //}
                                    //КОНЕЦ добавил блок 25.01.2026_#4
                                }
                            }

                            if (vremgoodsList.Count > 0)
                            {
                                //добавил блок 17.04.2026
                                if (vremgoodsList.Count > 1)
                                {
                                    //Найдено несколько позиций в документе, соответствующих отсканированному штрихкоду.
                                    //В приоритете будет выбрана позиция где CountReal=0,
                                    //а если нет таких, следующие будут те, в которых план > факта,
                                    //а потом уже те котрые собраны полностью, при условии, что такие существуют.
                                    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                    vremgoodsList = vremgoodsList
                                        //.OrderBy(c => c.Count_Real != 0)
                                        //06.05.2026 добавил SmartSortByCountReal()
                                        .SmartSortByCountReal()
                                        .ToList_Ext();

                                }
                                //КОНЕЦ добавил блок 17.04.2026

                                posIndex = 0;
                                adapter.Insert(vremgoodsList[0], 0);
                                Finded = true;
                                //добавил 11.12.2024_#3
                                FindedProduct = true;
                            }
                            else
                            {
                                //добавил блок 11.12.2024_#3
                                if (!FindedProduct)
                                {
                                    //добавил блок 15.09.2025
                                    //02.10.2025_#2 закоментировал блок
                                    //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                    //{
                                    //    //29.08.2025 заменил имя метода Count на Count_Ext
                                    //    if (dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    //    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    //    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)
                                    //    .Where(c => c.GoodF == goods[0].GoodF &&
                                    //            c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                    //            c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).Count_Ext() > 0)
                                    //    {
                                    //        FindedProduct = true;
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                    //КОНЕЦ добавил блок 15.09.2025
                                    //29.08.2025 заменил имя метода Count на Count_Ext
                                    if (dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                    .Include(c => c.Cell).Include(c => c.Good).Include(c => c.User)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF &&
                                     c.GoodF == goods[0].GoodF).Count_Ext() > 0)
                                    {
                                        FindedProduct = true;
                                    }
                                    //добавил блок 15.09.2025
                                    //02.10.2025_#2 закоментировал блок
                                    //}
                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                    //КОНЕЦ добавил блок 15.09.2025
                                }
                                //КОНЕЦ добавил блок 11.12.2024_#3
                            }

                            #endregion
                        }

                        //если найден в документе
                        if (Finded)
                        {
                            //ProductsListView.SetItemChecked(posIndex, true);

                            #region найден в документе

                            //добавил блок 27.05.2024
                            if (cs.BaseClass.preferences.GetBoolean("ForTypesDocuments_Outcoming_SpecifiedBarcode", false) && cs.BaseClass.currentDocHead?.DocType == 2)
                            {
                                if (!string.IsNullOrWhiteSpace(adapter.current?.Field_2))
                                {
                                    if (adapter.current.Field_2.Length > 5)
                                    {
                                        if (adapter.current.Field_2.Substring(0, 5) == "barc:")
                                        {
                                            string barcodeFiels_2 = adapter.current.Field_2.Substring(5).TrimEnd();
                                            if (barcodeFiels_2 != barcode)
                                            {
                                                cs.ScanerInit.ActiveScaner(false);
                                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                    .SetMessage(/*"Позиция товара в документе найдена, но указанный учетной системой штрихкод не соответсвует текущему"*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message12))
                                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                    .SetCancelable(false)
                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                        (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                    .Show();
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                            //КОНЕЦ добавил блок 27.05.2024

                            //читаю с настроек, режим автосканирования
                            bool modeautoscan = cs.BaseClass.preferences.GetBoolean("ModeAutoScanCheckBox", false);
                            if (ScanGS1_128)
                            {
                                if (modeautoscan)
                                {
                                    cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                    modeautoscan = false;
                                }
                            }
                            //добавил блок 08.09.2025_#1
                            else
                            {
                                if (cs.BaseClass.statusBarcodeModel.StatusProduct)
                                {
                                    modeautoscan = false;
                                }
                                //добавил блок 15.09.2025
                                else
                                {
                                    //добавил блок 15.09.2025
                                    //02.10.2025_#2 заменил  UsedAccountingParties на UsedAccountingParties_NEW
                                    if (cs.Settings.AccountingPartiesClass.UsedAccountingParties_NEW() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                    {
                                        modeautoscan = false;
                                    }
                                    //КОНЕЦ добавил блок 15.09.2025
                                }
                                //КОНЕЦ добавил блок 15.09.2025
                            }
                            //КОНЕЦ добавил блок 08.09.2025_#1

                            //добавил блок 21.05.2024
                            if (cs.BaseClass.preferences.GetBoolean("ForTypesDocuments_Incoming_ExpirationDate", false) && cs.BaseClass.currentDocHead?.DocType == 1 && goods[0].Field_2 == ExpirationDateDialog.MarkerExpirationDate)
                            {
                                if (modeautoscan)
                                {
                                    //cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                    modeautoscan = false;
                                }
                            }
                            //КОНЕЦ добавил блок 21.05.2024

                            //добавил блок 15.09.2025
                            //02.10.2025_#2 заменил  UsedAccountingParties на UsedAccountingParties_NEW
                            if (cs.Settings.AccountingPartiesClass.UsedAccountingParties_NEW() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                            {
                                if (modeautoscan)
                                {
                                    modeautoscan = false;
                                }
                            }
                            //КОНЕЦ добавил блок 15.09.2025

                            //добавил блок 21.01.2026_#1
                            if (Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.IsWaveAssemblyDocument())
                            {
                                if (modeautoscan)
                                {
                                    modeautoscan = false;
                                }
                            }
                            //КОНЕЦ добавил блок 21.01.2026_#1

                            //можно ли превышать плановое количество в документе
                            bool planPlus = false;

                            #region читаю с настроек, можно ли превышать плановое количество в документе

                            switch (cs.BaseClass.currentDocHead.DocType)
                            {
                                case 1:
                                    planPlus = cs.BaseClass.preferences.GetBoolean("InCommingPlanDocCheckBox", false);
                                    break;
                                case 2:
                                    planPlus = cs.BaseClass.preferences.GetBoolean("OutCommingPlanDocCheckBox", false);
                                    break;
                                case 3:
                                    planPlus = cs.BaseClass.preferences.GetBoolean("InventoryPlanDocCheckBox", false);
                                    break;
                                case 4:
                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        planPlus = false;
                                    }
                                    else
                                    {
                                        //добавил блок 12.03.2025_#3
                                        if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.SubDocType == "selling")
                                        {
                                            planPlus = cs.BaseClass.preferences.GetBoolean("ImplementationPlanDocCheckBox", false);
                                        }
                                        else
                                        {
                                            //23.01.2026 добавил блок
                                            if (Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.IsWaveAssemblyDocument(true))
                                            {
                                                planPlus = cs.BaseClass.preferences.GetBoolean("WavePlanDocCheckBox", false);
                                            }
                                            else
                                            {
                                                //КОНЕЦ 23.01.2026 добавил блок
                                                //добавил блок 07.08.2025_#1
                                                if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.isAllowForcedCompletion && cs.BaseClass.currentDocHead.SubDocType != "selling" && !cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                {
                                                    planPlus = true;
                                                }
                                                else
                                                {
                                                    //КОНЕЦ добавил блок 07.08.2025_#1
                                                    //КОНЕЦ добавил блок 12.03.2025_#3
                                                    planPlus = cs.BaseClass.preferences.GetBoolean("MovingPlanDocCheckBox", false);
                                                }
                                                //23.01.2026 добавил блок
                                            }
                                            //КОНЕЦ 23.01.2026 добавил блок
                                        }
                                    }
                                    break;
                                case 5:
                                    planPlus = cs.BaseClass.preferences.GetBoolean("WriteOffPlanDocCheckBox", false);
                                    break;
                                case 6:
                                    planPlus = cs.BaseClass.preferences.GetBoolean("ReturnPlanDocCheckBox", false);
                                    break;
                                //добавил блок 26.08.2025
                                case 9:
                                    planPlus = cs.BaseClass.preferences.GetBoolean("ProductionPlanDocCheckBox", false);
                                    break;
                                //КОНЕЦ добавил блок 26.08.2025
                                //добавил блок 03.01.2026
                                case 10:
                                    planPlus = cs.BaseClass.preferences.GetBoolean("WavePlanDocCheckBox", false);
                                    break;
                                    //КОНЕЦ добавил блок 03.01.2026
                            }

                            #endregion

                            #region если включен режим автосканирования и editFactCardProduct==false
                            if (modeautoscan && !editFactCardProduct)
                            {
                                //добавил блок 04.08.2025_#1
                                if (barcodes == null)
                                {
                                    //cs.ScanerInit.ActiveScaner(false);
                                    //cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                    //new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                    //    .SetMessage("Режим автосканирования будет отключен")
                                    //    .SetTitle("Сканер")
                                    //    .SetCancelable(false)
                                    //    .SetPositiveButton("OK",
                                    //        (senderAlert, args) => 
                                    //        {
                                    //            cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                    //            cs.ScanerInit.ActiveScaner(true);
                                    //        })
                                    //    .Show();
                                    //return;
                                    //cs.AlerrtDialogs.AlertDialogs.AlertDialog_V7(this, "Режим автосканирования будет отключен", "Сканер", false);
                                    cs.Toasts.aToast.ShowToast(this, /*"Режим автосканирования был отключен"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message49),
                                        ToastLength.Long, true);
                                    cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                    barcodeScanOperation(dataBase, goods, barcodes, barcode, statusBarcodeModel);
                                    return;
                                }
                                else
                                {
                                    //КОНЕЦ добавил блок 04.08.2025_#1
                                    if (barcodes[0].Count == 0)
                                    {
                                        //cs.ScanerInit.ActiveScaner(false);
                                        //cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                        //new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                        //    .SetMessage("Режим автосканирования будет отключен")
                                        //    .SetTitle("Сканер")
                                        //    .SetCancelable(false)
                                        //    .SetPositiveButton("OK",
                                        //        (senderAlert, args) => 
                                        //        {
                                        //            cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                        //            cs.ScanerInit.ActiveScaner(true);
                                        //        })
                                        //    .Show();
                                        //return;
                                        //cs.AlerrtDialogs.AlertDialogs.AlertDialog_V7(this, "Режим автосканирования будет отключен", "Сканер", false);
                                        cs.Toasts.aToast.ShowToast(this, /*"Режим автосканирования был отключен"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message49),
                                            ToastLength.Long, true);
                                        cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                        barcodeScanOperation(dataBase, goods, barcodes, barcode, statusBarcodeModel);
                                        return;
                                    }
                                    //добавил блок 04.08.2025_#1
                                }
                                //КОНЕЦ добавил блок 04.08.2025_#1
                                try
                                {
                                    //добавил строку 17.09.2025
                                    LinqExtensions.WriteErrorToFile = false;
                                    //var tlist = dataBase.dataContext.DocDetails
                                    //    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)
                                    //    .Where(c => c.GoodF == adapter.current.GoodF)
                                    //    .Where(c => c.Cell.BarcodeCell == cs.BaseClass.currentNameCell).First_Ext();
                                    DocDetails tlist = null;

                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                        //{
                                        //    //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //    tlist = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory).ThenInclude(c => c.Cell)
                                        //            .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)
                                        //            .Where(c => c.GoodF == adapter.current.GoodF)
                                        //            //01.09.2025 заменил имя метода First на First_Ext
                                        //            .Where(c => c.CellF == null &&
                                        //            c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                        //            c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)
                                        //            ).First_Ext();
                                        //}
                                        //else
                                        //{
                                        //КОНЕЦ добавил блок 15.09.2025
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //01.09.2025 заменил имя метода First на First_Ext    
                                        tlist = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory).ThenInclude(c => c.Cell)
                                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF &&
                                                     c.GoodF == adapter.current.GoodF &&
                                                     c.CellF == null).First_Ext();
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //}
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                    }
                                    else
                                    {
                                        //добавил блок 19.06.2025
                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                        {
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    tlist = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory).ThenInclude(c => c.Cell)
                                            //        .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)
                                            //        .Where(c => c.GoodF == adapter.current.GoodF)
                                            //        //01.09.2025 заменил имя метода First на First_Ext
                                            //        .Where(c => c.CellF == null &&
                                            //        c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //        c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).First_Ext();
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода First на First_Ext
                                            tlist = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory).ThenInclude(c => c.Cell)
                                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF &&
                                                    c.GoodF == adapter.current.GoodF &&
                                                    c.CellF == null).First_Ext();
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                        else
                                        {
                                            //КОНЕЦ добавил блок 19.06.2025
                                            try
                                            {
                                                //добавил блок 15.09.2025
                                                //02.10.2025_#2 закоментировал блок
                                                //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                //{
                                                //    tlist = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                                //   .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)
                                                //   .Where(c => c.GoodF == adapter.current.GoodF)
                                                //   //01.09.2025 заменил имя метода First на First_Ext
                                                //   .Where(c => c.Cell.Name == cs.BaseClass.currentNameCell &&
                                                //    c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                //    c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).First_Ext();
                                                //}
                                                //else
                                                //{
                                                //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                //КОНЕЦ добавил блок 15.09.2025
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                tlist = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                                .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF &&
                                                 c.GoodF == adapter.current.GoodF &&
                                                 c.Cell.Name == cs.BaseClass.currentNameCell).First_Ext();
                                                //добавил блок 15.09.2025
                                                //02.10.2025_#2 закоментировал блок
                                                //}
                                                //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                //КОНЕЦ добавил блок 15.09.2025
                                            }
                                            catch
                                            {
                                            }

                                            if (tlist == null)
                                            {
                                                //добавил блок 15.09.2025
                                                //02.10.2025_#2 закоментировал блок
                                                //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                //{
                                                //    tlist = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                                //    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF)
                                                //    .Where(c => c.GoodF == adapter.current.GoodF)
                                                //    //01.09.2025 заменил имя метода First на First_Ext
                                                //    .Where(c => c.CellF == null &&
                                                //    c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                //    c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).First_Ext();
                                                //}
                                                //else
                                                //{
                                                //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                //КОНЕЦ добавил блок 15.09.2025
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                tlist = dataBase.dataContext.DocDetails.Include(c => c.ScanHistory)
                                                .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF &&
                                                 c.GoodF == adapter.current.GoodF &&
                                                 c.CellF == null).First_Ext();
                                                //добавил блок 15.09.2025
                                                //02.10.2025_#2 закоментировал блок
                                                //}
                                                //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                //КОНЕЦ добавил блок 15.09.2025
                                            }

                                            if (tlist != null)
                                            {
                                                //exsistCell = true;
                                            }
                                        }
                                    }
                                    //добавил строку 17.09.2025
                                    LinqExtensions.WriteErrorToFile = true;
                                }
                                catch
                                {
                                    //добавил строку 17.09.2025
                                    LinqExtensions.WriteErrorToFile = true;
                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        cs.ScanerInit.ActiveScaner(false);
                                        cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                            .SetMessage(Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message11))
                                            .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                            .SetCancelable(false)
                                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                            .Show();
                                        return;

                                    }
                                    else
                                    {
                                        SmartStoreData.DocDetails adapterCurrent = new SmartStoreData.DocDetails();
                                        adapterCurrent.Bad_price = false;
                                        adapter.current.Count_Doc = 0;
                                        if (cs.BaseClass.currentIdCell > 0)
                                        {
                                            adapterCurrent.CellF = cs.BaseClass.currentIdCell;
                                            if (!cs.BaseClass.preferences.GetBoolean("StrongCellCheckBox", true) &&
                                                cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                                            {
                                                //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                                adapterCurrent.Cell = dataBase.dataContext.Cell/*.AsNoTracking()*/
                                                    .Where(c => c.CellF == cs.BaseClass.currentIdCell).FirstOrDefault_Ext();
                                            }
                                        }

                                        //флаг ручного изменения истории
                                        adapterCurrent.Change_history = false;
                                        adapterCurrent.Count_Doc = 0;
                                        if (statusBarcodeModel.StatusProduct)
                                        {
                                            adapterCurrent.Count_Real = 0;
                                        }
                                        else
                                        {
                                            adapterCurrent.Count_Real = 0;
                                        }

                                        adapterCurrent.CreateDate = DateTime.Now.Ticks;
                                        adapterCurrent.DocHeadF = cs.BaseClass.currentDocHead.DocHeadF;
                                        if (statusBarcodeModel.StatusProduct)
                                        {
                                            try
                                            {
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                adapterCurrent.GoodF = goods.First_Ext().GoodF;
                                            }
                                            catch
                                            {
                                                adapterCurrent.GoodF = statusBarcodeModel.CodeProduct;
                                            }
                                        }
                                        else
                                        {
                                            adapterCurrent.GoodF = goods[0].GoodF;
                                        }

                                        ////////adapterCurrent.Have_comment = false;
                                        ////////adapterCurrent.Have_spec_comment = false;
                                        //ручной ввод
                                        adapterCurrent.Hend_enter = cs.BaseClass.HendEnter;
                                        adapterCurrent.DocDetailsF = Guid.NewGuid();
                                        //////adapterCurrent.Price = goods[0].Price;

                                        ////////adapterCurrent.SummDoc = null;
                                        ////////adapterCurrent.SummReal = null;
                                        adapterCurrent.UpdatedFromTSD = true;
                                        adapterCurrent.UpdateFrom1C = false;
                                        adapterCurrent.UserF = cs.BaseClass.currentIdUser;
                                        ////////adapterCurrent.identUser = cs.BaseClass.identUser;
                                        if (SmartStoreData.SourceDataBase.LocalDB == SmartStoreData.TypeDatabaseEnum.local)
                                        {
                                            adapterCurrent.id = 1;
                                            try
                                            {
                                                LinqExtensions.WriteErrorToFile = false;
                                                //29.08.2025 заменил .Max на .Max_Ext
                                                adapterCurrent.id = dataBase.dataContext.DocDetails.AsNoTracking().Max_Ext(c => c.id);
                                                LinqExtensions.WriteErrorToFile = true;
                                            }
                                            catch
                                            {
                                            }
                                        }

                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                        //{
                                        //    adapterCurrent.BatchNumber = statusBarcodeModel.compositeBarcodeModel.batch_number;
                                        //    adapterCurrent.ViewPack = cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack);
                                        //}
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025

                                        dataBase.dataContext.DocDetails.Add(adapterCurrent);
                                        int y = dataBase.dataContext.SaveChanges();
                                        if ((y < 1) && (y > 2))
                                        {
                                            cs.ScanerInit.ActiveScaner(false);
                                            cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                            new AlertDialog.Builder(this, Resource.Style.DialogTheme)

                                                //.SetMessage("Не удалось обновить информацию о товаре" +
                                                //            adapter.current.GoodF + " в текущем документе (" + y + ")")
                                                .SetMessage(string.Format(Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message50), adapter.current.GoodF, y))
                                                .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                .SetCancelable(false)
                                                .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                    (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                .Show();
                                            return;
                                        }

                                    }
                                }
                                //добавил блок 17.09.2025
                                finally
                                {
                                    LinqExtensions.WriteErrorToFile = true;
                                }
                                //КОНЕЦ добавил блок 15.09.2025

                                var details = adapter.current;

                                List<SmartStoreData.DocDetails> list = new List<SmartStoreData.DocDetails>();
                                if (!SmartStoreData.SourceDataBase.showDocDetails)
                                {
                                    //добавил 07.03.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                        //{
                                        //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        //    list = dataBase.dataContext.DocDetails.Include(c => c.Cell).Include(c => c.ScanHistory).ThenInclude(c => c.Cell)
                                        //.Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == details.GoodF &&
                                        //c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                        //c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                        //.ToList_Ext();
                                        //}
                                        //else
                                        //{
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        list = dataBase.dataContext.DocDetails.Include(c => c.Cell).Include(c => c.ScanHistory).ThenInclude(c => c.Cell)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == details.GoodF)
                                    .ToList_Ext();
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //}
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                    }
                                    else
                                    {
                                        //добавил блок 19.06.2025
                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                        {
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    list = dataBase.dataContext.DocDetails.Include(c => c.Cell).Include(c => c.ScanHistory).ThenInclude(c => c.Cell)
                                            //           .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == details.GoodF &&
                                            //                  c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //                  c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //           .ToList_Ext();
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            list = dataBase.dataContext.DocDetails.Include(c => c.Cell).Include(c => c.ScanHistory).ThenInclude(c => c.Cell)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == details.GoodF)
                                    .ToList_Ext();
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                        else
                                        {
                                            //КОНЕЦ добавил блок 19.06.2025
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    list = dataBase.dataContext.DocDetails.Include(c => c.Cell).Include(c => c.ScanHistory)
                                            //                  .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == details.GoodF &&
                                            //                  c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //                  c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //                  .ToList_Ext();
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            list = dataBase.dataContext.DocDetails.Include(c => c.Cell).Include(c => c.ScanHistory)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == details.GoodF)
                                    .ToList_Ext();
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                    }
                                }
                                else
                                {
                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                        //{
                                        //    //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        //    list = dataBase.dataContext.DocDetails.Include(c => c.Cell).Include(c => c.ScanHistory).ThenInclude(c => c.Cell)
                                        //     .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == details.GoodF &&
                                        //                      c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                        //                      c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                        //     .ToList_Ext();
                                        //}
                                        //else
                                        //{
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        list = dataBase.dataContext.DocDetails.Include(c => c.Cell).Include(c => c.ScanHistory).ThenInclude(c => c.Cell)
                                         .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == details.GoodF)
                                         .ToList_Ext();
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //}
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                    }
                                    else
                                    {
                                        //добавил блок 19.06.2025
                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                        {
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    list = dataBase.dataContext.DocDetails.Include(c => c.Cell).Include(c => c.ScanHistory).ThenInclude(c => c.Cell)
                                            // .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == details.GoodF &&
                                            //                  c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //                  c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //                  .ToList_Ext();
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            list = dataBase.dataContext.DocDetails.Include(c => c.Cell).Include(c => c.ScanHistory).ThenInclude(c => c.Cell)
                                         .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == details.GoodF)
                                         .ToList_Ext();
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                        else
                                        {
                                            //КОНЕЦ добавил блок 19.06.2025
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    list = dataBase.dataContext.DocDetails.Include(c => c.Cell).Include(c => c.ScanHistory)
                                            //                  .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == details.GoodF &&
                                            //                  c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                            //                  c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack))
                                            //                  .ToList_Ext();
                                            //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //    var teklist = list.Where(c => c.CellF == cs.BaseClass.currentIdCell).ToList_Ext();
                                            //    if (teklist.Count == 0)
                                            //    {
                                            //        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            //        list = list.Where(c => c.CellF == null).ToList_Ext();
                                            //    }
                                            //    else
                                            //    {
                                            //        list = teklist;
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            list = dataBase.dataContext.DocDetails.Include(c => c.Cell).Include(c => c.ScanHistory)
                                    .Where(c => c.DocHeadF == cs.BaseClass.currentDocHead.DocHeadF && c.GoodF == details.GoodF)
                                    .ToList_Ext();
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            var teklist = list.Where(c => c.CellF == cs.BaseClass.currentIdCell).ToList_Ext();
                                            if (teklist.Count == 0)
                                            {
                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                list = list.Where(c => c.CellF == null).ToList_Ext();
                                            }
                                            else
                                            {
                                                list = teklist;
                                            }
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                    }
                                }

                                #region актуализирую историю

                                List<SmartStoreData.ScanHistory> ActualListHistory = new List<SmartStoreData.ScanHistory>();
                                foreach (var itemhistory in list)
                                {
                                    if (itemhistory.ScanHistory != null)
                                    {
                                        if (itemhistory.ScanHistory.Count > 0)
                                        {
                                            ActualListHistory.AddRange(itemhistory.ScanHistory);
                                        }
                                    }
                                }

                                #endregion

                                List<SmartStoreData.DocDetails> tyuio = new List<DocDetails>();
                                if (list.Count > 0)
                                {
                                    double sumCountReal = 0;
                                    if (statusBarcodeModel.StatusProduct)
                                    {
                                        if (!planPlus)
                                        {
                                            foreach (var item in list)
                                            {
                                                List<SmartStoreData.ScanHistory> tyui = new List<SmartStoreData.ScanHistory>();
                                                //добавил 07.03.2024
                                                if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                {
                                                    //добавил блок 15.09.2025
                                                    //02.10.2025_#2 закоментировал блок
                                                    //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                    //{
                                                    //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                    //    tyui = dataBase.dataContext.ScanHistory.Include(c => c.Cell).AsNoTracking()
                                                    //.Where(c => c.DocDetailsF == item.DocDetailsF &&
                                                    //          c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                    //          c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).ToList_Ext();
                                                    //}
                                                    //else
                                                    //{
                                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                    //КОНЕЦ добавил блок 15.09.2025
                                                    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                    tyui = dataBase.dataContext.ScanHistory.Include(c => c.Cell).AsNoTracking()
                                                .Where(c => c.DocDetailsF == item.DocDetailsF).ToList_Ext();
                                                    //добавил блок 15.09.2025
                                                    //02.10.2025_#2 закоментировал блок
                                                    //}
                                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                    //КОНЕЦ добавил блок 15.09.2025
                                                }
                                                else
                                                {
                                                    //добавил блок 19.06.2025
                                                    if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                                    {
                                                        //добавил блок 15.09.2025
                                                        //02.10.2025_#2 закоментировал блок
                                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                        //{
                                                        //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                        //    tyui = dataBase.dataContext.ScanHistory.Include(c => c.Cell).AsNoTracking()
                                                        //           .Where(c => c.DocDetailsF == item.DocDetailsF &&
                                                        //           c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                        //           c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).ToList_Ext();
                                                        //}
                                                        //else
                                                        //{
                                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                        tyui = dataBase.dataContext.ScanHistory.Include(c => c.Cell).AsNoTracking()
                                                               .Where(c => c.DocDetailsF == item.DocDetailsF).ToList_Ext();
                                                        //добавил блок 15.09.2025
                                                        //02.10.2025_#2 закоментировал блок
                                                        //}
                                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                    }
                                                    else
                                                    {
                                                        //КОНЕЦ добавил блок 19.06.2025
                                                        //добавил блок 15.09.2025
                                                        //02.10.2025_#2 закоментировал блок
                                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                        //{
                                                        //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                        //    tyui = dataBase.dataContext.ScanHistory.AsNoTracking()
                                                        //           .Where(c => c.DocDetailsF == item.DocDetailsF &&
                                                        //           c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                        //           c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).ToList_Ext();
                                                        //}
                                                        //else
                                                        //{
                                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                        tyui = dataBase.dataContext.ScanHistory.AsNoTracking()
                                                               .Where(c => c.DocDetailsF == item.DocDetailsF).ToList_Ext();
                                                        //добавил блок 15.09.2025
                                                        //02.10.2025_#2 закоментировал блок
                                                        //}
                                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                    }
                                                }
                                                if (cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                                                {
                                                    //добавил 07.02.2024
                                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                    {
                                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                                        tyui = tyui;
                                                    }
                                                    else
                                                    {
                                                        //добавил блок 19.06.2025
                                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                                        {
                                                            tyui = tyui;
                                                        }
                                                        else
                                                        {
                                                            //КОНЕЦ добавил блок 19.06.2025
                                                            if (cs.BaseClass.currentIdCell.HasValue)
                                                            {
                                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                tyui = tyui.Where(c => c.CellF == cs.BaseClass.currentIdCell.Value)
                                                                    .ToList_Ext();
                                                            }
                                                        }
                                                    }
                                                }

                                                foreach (var item2 in tyui)
                                                {
                                                    sumCountReal += item2.Count;
                                                }
                                            }

                                            sumCountReal +=
                                                System.Math.Round(Convert.ToDouble(statusBarcodeModel.WeightProduct), 3);
                                        }
                                        else
                                        {
                                            sumCountReal = list.Sum(c => c.Count_Real) +
                                                           System.Math.Round(
                                                               Convert.ToDouble(statusBarcodeModel.WeightProduct),
                                                               3);
                                        }
                                    }
                                    else
                                    {
                                        if (!planPlus)
                                        {
                                            foreach (var item in list)
                                            {
                                                List<SmartStoreData.ScanHistory> tyui = new List<SmartStoreData.ScanHistory>();

                                                //добавил 07.03.2024
                                                if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                {
                                                    //добавил блок 15.09.2025
                                                    //02.10.2025_#2 закоментировал блок
                                                    //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                    //{
                                                    //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                    //    tyui = dataBase.dataContext.ScanHistory.Include(c => c.Cell).AsNoTracking()
                                                    //.Where(c => c.DocDetailsF == item.DocDetailsF &&
                                                    //c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                    //               c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).ToList_Ext();
                                                    //}
                                                    //else
                                                    //{
                                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                    //КОНЕЦ добавил блок 15.09.2025
                                                    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                    tyui = dataBase.dataContext.ScanHistory.Include(c => c.Cell).AsNoTracking()
                                                .Where(c => c.DocDetailsF == item.DocDetailsF).ToList_Ext();
                                                    //добавил блок 15.09.2025
                                                    //02.10.2025_#2 закоментировал блок
                                                    //}
                                                    //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                    //КОНЕЦ добавил блок 15.09.2025
                                                }
                                                else
                                                {
                                                    //добавил блок 19.06.2025
                                                    if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                                    {
                                                        //добавил блок 15.09.2025
                                                        //02.10.2025_#2 закоментировал блок
                                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                        //{
                                                        //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                        //    tyui = dataBase.dataContext.ScanHistory.Include(c => c.Cell).AsNoTracking()
                                                        //           .Where(c => c.DocDetailsF == item.DocDetailsF &&
                                                        //           c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                        //           c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).ToList_Ext();
                                                        //}
                                                        //else
                                                        //{
                                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                        tyui = dataBase.dataContext.ScanHistory.Include(c => c.Cell).AsNoTracking()
                                                               .Where(c => c.DocDetailsF == item.DocDetailsF).ToList_Ext();
                                                        //добавил блок 15.09.2025
                                                        //02.10.2025_#2 закоментировал блок
                                                        //}
                                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                    }
                                                    else
                                                    {
                                                        //КОНЕЦ добавил блок 19.06.2025
                                                        //добавил блок 15.09.2025
                                                        //02.10.2025_#2 закоментировал блок
                                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                        //{
                                                        //    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                        //    tyui = dataBase.dataContext.ScanHistory.AsNoTracking()
                                                        //           .Where(c => c.DocDetailsF == item.DocDetailsF &&
                                                        //           c.BatchNumber == statusBarcodeModel.compositeBarcodeModel.batch_number &&
                                                        //           c.ViewPack == cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack)).ToList_Ext();
                                                        //}
                                                        //else
                                                        //{
                                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                        tyui = dataBase.dataContext.ScanHistory.AsNoTracking()
                                                               .Where(c => c.DocDetailsF == item.DocDetailsF).ToList_Ext();
                                                        //добавил блок 15.09.2025
                                                        //02.10.2025_#2 закоментировал блок
                                                        //}
                                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                    }
                                                }
                                                if (cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                                                {
                                                    //добавил 07.02.2024
                                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                    {
                                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                                        tyui = tyui;
                                                    }
                                                    else
                                                    {
                                                        //добавил блок 19.06.2025
                                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                                        {
                                                            tyui = tyui;
                                                        }
                                                        else
                                                        {
                                                            //КОНЕЦ добавил блок 19.06.2025
                                                            if (cs.BaseClass.currentIdCell.HasValue)
                                                            {
                                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                                tyui = tyui.Where(c => c.CellF == cs.BaseClass.currentIdCell.Value)
                                                                    .ToList_Ext();
                                                            }
                                                        }
                                                    }
                                                }

                                                foreach (var item2 in tyui)
                                                {
                                                    sumCountReal += item2.Count;
                                                }
                                            }

                                            sumCountReal += barcodes[0].Count;
                                        }
                                        else
                                        {
                                            sumCountReal = list.Sum(c => c.Count_Real) + barcodes[0].Count;
                                        }
                                    }

                                    if ((statusBarcodeModel.StatusProduct) || (goods[0].Unit == "weight" && cs.BaseClass.preferences.GetBoolean("WeightUsed", false)))
                                    {
                                        //если позиция в документе с плановым количеством больше 0
                                        if (ListDocDetailsActivity.PlanOrNewDocu)
                                        {
                                            double procent = 0;
                                            //01.09.2025 заменил имя метода First на First_Ext
                                            double maxValueDocCount = list.First_Ext().Count_Doc;
                                            //01.09.2025 заменил имя метода First на First_Ext
                                            double minValueDocCount = list.First_Ext().Count_Doc;

                                            foreach (var item in cs.BaseClass.weightSettings.ListRound)
                                            {
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                if ((item.RoundMax >= list.First_Ext().Count_Doc) &&
                                                    //01.09.2025 заменил имя метода First на First_Ext
                                                    (item.RoundMin <= list.First_Ext().Count_Doc))
                                                {
                                                    procent = item.Percent;
                                                    maxValueDocCount += maxValueDocCount * procent;
                                                    maxValueDocCount = System.Math.Round(maxValueDocCount, 3);
                                                    minValueDocCount -= minValueDocCount * procent;
                                                    minValueDocCount = System.Math.Round(minValueDocCount, 3);
                                                    break;
                                                }
                                            }

                                            if ((sumCountReal > maxValueDocCount) &&
                                                (ListDocDetailsActivity.PlanOrNewDocu) && (!planPlus) &&
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                (list.First_Ext().Count_Doc != 0))
                                            {
                                                this.RunOnUiThread(() =>
                                                {
                                                    cs.ScanerInit.ActiveScaner(false);
                                                    cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                    new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                        .SetMessage(
                                                            /*"Запрещено превышать плановое количество товаров в документе"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message34))
                                                        .SetTitle(/*"Разрешения"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message33))
                                                        .SetCancelable(false)
                                                        //.SetIcon(Resource.Drawable.SS_icon2)
                                                        .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                            (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                        .Show();
                                                });
                                                return;
                                            }

                                        }
                                    }
                                    else
                                    {
                                        ///если запрещено превышать плановое количество товара в документе 
                                        ///и документ плановый, а не новый
                                        if ((!planPlus) && (ListDocDetailsActivity.PlanOrNewDocu))
                                        {
                                            //01.09.2025 заменил имя метода First на First_Ext
                                            if ((sumCountReal > list.First_Ext().Count_Doc) && (list.First_Ext().Count_Doc != 0))
                                            {
                                                this.RunOnUiThread(() =>
                                                {
                                                    cs.ScanerInit.ActiveScaner(false);
                                                    cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                    new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                        .SetMessage(
                                                            /*"Запрещено превышать плановое количество товаров в документе"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message34))
                                                        .SetTitle(/*"Разрешения"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message33))
                                                        .SetCancelable(false)
                                                        //.SetIcon(Resource.Drawable.SS_icon2)
                                                        .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                            (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                        .Show();
                                                });
                                                return;
                                            }
                                        }
                                    }

                                    var ttttttt = cs.BaseClass.currentNameCell;
                                    ttttttt = ttttttt;

                                    tyuio = list;
                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                        tyuio = tyuio.Where(c => c.CellF == null).ToList_Ext();
                                    }
                                    else
                                    {
                                        //добавил блок 19.06.2025
                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                        {
                                            //01.09.2025 заменил имя метода ToList на ToList_Ext
                                            tyuio = tyuio.Where(c => c.CellF == null).ToList_Ext();
                                        }
                                        else
                                        {
                                            //КОНЕЦ добавил блок 19.06.2025
                                            if (cs.BaseClass.currentNameCell == null)
                                            {
                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                tyuio = tyuio.Where(c => c.CellF == null).ToList_Ext();
                                            }
                                            else
                                            {
                                                //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                tyuio = tyuio.Where(c => c.Cell != null)
                                                    .Where(c => c.Cell.Name == cs.BaseClass.currentNameCell).ToList_Ext();
                                                if (tyuio.Count == 0)
                                                {
                                                    tyuio = list;
                                                    //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                    tyuio = tyuio.Where(c => c.CellF == null).ToList_Ext();
                                                }
                                            }
                                        }
                                    }
                                    if (statusBarcodeModel.StatusProduct)
                                    {
                                        //01.09.2025 заменил имя метода First на First_Ext
                                        tyuio.First_Ext().Count_Real +=
                                            System.Math.Round(Convert.ToDouble(statusBarcodeModel.WeightProduct), 3);
                                    }
                                    else
                                    {
                                        //01.09.2025 заменил имя метода First на First_Ext
                                        tyuio.First_Ext().Count_Real += System.Math.Round(barcodes[0].Count, 3);
                                    }

                                    //01.09.2025 заменил имя метода First на First_Ext
                                    tyuio.First_Ext().UpdatedFromTSD = true;
                                    //01.09.2025 заменил имя метода First на First_Ext
                                    tyuio.First_Ext().UserF = cs.BaseClass.currentIdUser;
                                    ////////tyuio.First_Ext().identUser = cs.BaseClass.identUser;

                                    //01.09.2025 заменил имя метода First на First_Ext
                                    if (!tyuio.First_Ext().Hend_enter)
                                    {
                                        //01.09.2025 заменил имя метода First на First_Ext
                                        tyuio.First_Ext().Hend_enter = cs.BaseClass.HendEnter;
                                    }
                                    //01.09.2025 заменил имя метода First на First_Ext
                                    tyuio.First_Ext().CreateDate = DateTime.Now.Ticks;
                                    //актуализирую историю
                                    adapter.current.ScanHistory = ActualListHistory;
                                    adapter.current.Count_Real = sumCountReal;
                                    adapter.current.Count_Doc = list.Sum(c => c.Count_Doc);

                                    if (!adapter.current.Hend_enter)
                                    {
                                        adapter.current.Hend_enter = cs.BaseClass.HendEnter;
                                    }

                                    adapter.current.UpdatedFromTSD = true;
                                    //01.09.2025 заменил имя метода First на First_Ext
                                    adapter.current.CreateDate = tyuio.First_Ext().CreateDate;
                                    if (!cs.BaseClass.preferences.GetBoolean("StrongCellCheckBox", true) &&
                                        cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                                    {
                                        //добавил 07.02.2024
                                        if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                        {
                                            //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                            adapter.current.CellF = null;
                                            adapter.current.Cell = null;
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    adapter.current.BatchNumber = statusBarcodeModel.compositeBarcodeModel.batch_number;
                                            //    adapter.current.ViewPack = cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack);
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                        else
                                        {
                                            //добавил блок 19.06.2025
                                            if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                            {
                                                adapter.current.CellF = null;
                                                adapter.current.Cell = null;
                                                //добавил блок 15.09.2025
                                                //02.10.2025_#2 закоментировал блок
                                                //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                //{
                                                //    adapter.current.BatchNumber = statusBarcodeModel.compositeBarcodeModel.batch_number;
                                                //    adapter.current.ViewPack = cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack);
                                                //}
                                                //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                //КОНЕЦ добавил блок 15.09.2025
                                            }
                                            else
                                            {
                                                //КОНЕЦ добавил блок 19.06.2025
                                                adapter.current.CellF = cs.BaseClass.currentIdCell;
                                                adapter.current.Cell = dataBase.dataContext.Cell/*.AsNoTracking()*/
                                                    //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                                    .Where(c => c.CellF == cs.BaseClass.currentIdCell).FirstOrDefault_Ext();
                                                //добавил блок 15.09.2025
                                                //02.10.2025_#2 закоментировал блок
                                                //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                //{
                                                //    adapter.current.BatchNumber = statusBarcodeModel.compositeBarcodeModel.batch_number;
                                                //    adapter.current.ViewPack = cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack);
                                                //}
                                                //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                                //КОНЕЦ добавил блок 15.09.2025
                                            }
                                        }
                                    }

                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        //ничего не делать
                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                        //{
                                        //    adapter.current.BatchNumber = statusBarcodeModel.compositeBarcodeModel.batch_number;
                                        //    adapter.current.ViewPack = cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack);
                                        //}
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                    }
                                    else
                                    {
                                        //добавил блок 19.06.2025
                                        if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                        {
                                            //ничего не делать
                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    adapter.current.BatchNumber = statusBarcodeModel.compositeBarcodeModel.batch_number;
                                            //    adapter.current.ViewPack = cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack);
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                        else
                                        {
                                            //КОНЕЦ добавил блок 19.06.2025
                                            try
                                            {
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                if (tyuio.First_Ext().CellF == null)
                                                {
                                                    //01.09.2025 заменил имя метода First на First_Ext
                                                    tyuio.First_Ext().CellF = cs.BaseClass.currentIdCell;
                                                    //01.09.2025 заменил имя метода First на First_Ext
                                                    if (tyuio.First_Ext().CellF != null)
                                                        //01.09.2025 заменил имя метода First на First_Ext
                                                        tyuio.First_Ext().Cell = dataBase.dataContext.Cell/*.AsNoTracking()*/
                                                            //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                                            .Where(c => c.CellF == cs.BaseClass.currentIdCell).FirstOrDefault_Ext();
                                                }
                                            }
                                            catch (System.Exception ex)
                                            {
                                                ex = ex;
                                            }

                                            if (adapter.current.CellF == null)
                                            {
                                                adapter.current.CellF = cs.BaseClass.currentIdCell;
                                                if (adapter.current.CellF != null)
                                                    adapter.current.Cell = dataBase.dataContext.Cell/*.AsNoTracking()*/
                                                        //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                                        .Where(c => c.CellF == cs.BaseClass.currentIdCell).FirstOrDefault_Ext();
                                            }

                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    adapter.current.BatchNumber = statusBarcodeModel.compositeBarcodeModel.batch_number;
                                            //    adapter.current.ViewPack = cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack);
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                        }
                                    }
                                    adapter.Update();
                                }
                                else
                                {
                                    bool newpos = false;

                                    //добавил 07.02.2024
                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                    {
                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                        cs.ScanerInit.ActiveScaner(false);
                                        cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                            .SetMessage(/*Товар не найден в документе*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message11))
                                            .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                            .SetCancelable(false)
                                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                            .Show();
                                        return;
                                    }
                                    else
                                    {
                                        //если новый (слепой) документ, тогда разрешаем добавлять новые позиции, в не зависимости от того, что стоит в настройках
                                        if (!ListDocDetailsActivity.PlanOrNewDocu)
                                        {
                                            newpos = true;
                                        }
                                        else
                                        {
                                            #region читаю с настроек, можно ли добавлять новые позиции в документ

                                            switch (cs.BaseClass.currentDocHead.DocType)
                                            {
                                                case 1:
                                                    newpos = cs.BaseClass.preferences.GetBoolean("InCommingDocCheckBox", true);
                                                    break;
                                                case 2:
                                                    newpos = cs.BaseClass.preferences.GetBoolean("OutCommingDocCheckBox", true);
                                                    break;
                                                case 3:
                                                    newpos = cs.BaseClass.preferences.GetBoolean("InventoryDocCheckBox", true);
                                                    break;
                                                case 4:
                                                    //добавил 07.02.2024
                                                    if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                                    {
                                                        //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                                        newpos = false;
                                                    }
                                                    else
                                                    {
                                                        newpos = cs.BaseClass.preferences.GetBoolean("MovingDocCheckBox", true);
                                                    }
                                                    break;
                                                case 5:
                                                    newpos = cs.BaseClass.preferences.GetBoolean("WriteOffDocCheckBox", true);
                                                    break;
                                                case 6:
                                                    newpos = cs.BaseClass.preferences.GetBoolean("ReturnDocCheckBox", true);
                                                    break;
                                                //добавил блок 26.08.2025
                                                case 9:
                                                    newpos = cs.BaseClass.preferences.GetBoolean("ProductionDocCheckBox", true);
                                                    break;
                                                //КОНЕЦ добавил блок 26.08.2025
                                                //добавил блок 03.01.2026
                                                case 10:
                                                    newpos = cs.BaseClass.preferences.GetBoolean("WaveDocCheckBox", false);
                                                    break;
                                                    //КОНЕЦ добавил блок 03.01.2026
                                            }

                                            #endregion
                                        }

                                        if (newpos)
                                        {
                                            adapter.current = new SmartStoreData.DocDetails();
                                            adapter.current.Bad_price = false;
                                            if (cs.BaseClass.currentIdCell > 0)
                                            {
                                                adapter.current.CellF = cs.BaseClass.currentIdCell;
                                                if (!cs.BaseClass.preferences.GetBoolean("StrongCellCheckBox", true) &&
                                                    cs.BaseClass.preferences.GetBoolean("CellCheckBox", false))
                                                {
                                                    adapter.current.Cell = dataBase.dataContext.Cell/*.AsNoTracking()*/
                                                        //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                                        .Where(c => c.CellF == cs.BaseClass.currentIdCell).FirstOrDefault_Ext();
                                                }
                                            }

                                            //флаг ручного изменения истории
                                            adapter.current.Change_history = false;
                                            adapter.current.Count_Doc = 0;
                                            if (statusBarcodeModel.StatusProduct)
                                            {
                                                adapter.current.Count_Real = Convert.ToDouble(statusBarcodeModel.WeightProduct);
                                            }
                                            else
                                            {
                                                adapter.current.Count_Real = barcodes[0].Count;
                                            }

                                            adapter.current.CreateDate = DateTime.Now.Ticks;
                                            adapter.current.DocHeadF = cs.BaseClass.currentDocHead.DocHeadF;
                                            adapter.current.GoodF = goods[0].GoodF;
                                            ////////adapter.current.Have_comment = false;
                                            ////////adapter.current.Have_spec_comment = false;
                                            //ручной ввод
                                            adapter.current.Hend_enter = cs.BaseClass.HendEnter;
                                            adapter.current.DocDetailsF = details.DocDetailsF;
                                            //////adapter.current.Price = goods[0].Price;
                                            ////////adapter.current.SummDoc = null;
                                            ////////adapter.current.SummReal = null;
                                            adapter.current.UpdatedFromTSD = true;
                                            adapter.current.UpdateFrom1C = false;
                                            adapter.current.UserF = cs.BaseClass.currentIdUser;

                                            //добавил условие 22.11.2024, т.к. при большом справочнике долго запрос выполнняется
                                            //желательно и для офлайн режима в будующем переделать, чтобы не требовался запрос
                                            if (SmartStoreData.SourceDataBase.LocalDB == TypeDatabaseEnum.local)
                                            {
                                                adapter.current.id = 1;
                                                try
                                                {
                                                    LinqExtensions.WriteErrorToFile = false;
                                                    //29.08.2025 заменил .Max на .Max_Ext
                                                    adapter.current.id = dataBase.dataContext.DocDetails.AsNoTracking().Max_Ext(c => c.id);
                                                    LinqExtensions.WriteErrorToFile = true;
                                                }
                                                catch
                                                {
                                                }
                                            }

                                            //добавил блок 15.09.2025
                                            //02.10.2025_#2 закоментировал блок
                                            //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                            //{
                                            //    adapter.current.BatchNumber = statusBarcodeModel.compositeBarcodeModel.batch_number;
                                            //    adapter.current.ViewPack = cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack);
                                            //}
                                            //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                            //КОНЕЦ добавил блок 15.09.2025
                                            dataBase.dataContext.DocDetails.Add(adapter.current);
                                            adapter.current.Count_Real += adapter.current.Count_Real;

                                            if (!adapter.current.Hend_enter)
                                            {
                                                adapter.current.Hend_enter = cs.BaseClass.HendEnter;
                                            }

                                            adapter.current.UpdatedFromTSD = true;
                                            adapter.Update();
                                        }
                                        else
                                        {
                                            this.RunOnUiThread(() =>
                                            {
                                                cs.ScanerInit.ActiveScaner(false);
                                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                    .SetMessage(/*"Запрещено добавлять новые позиции в документ"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message51))
                                                    .SetTitle(/*"Разрешения"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message33))
                                                    .SetCancelable(false)
                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                        (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                    .Show();
                                            });
                                            return;
                                        }
                                    }
                                }

                                try
                                {
                                    int y = dataBase.dataContext.SaveChanges();
                                    if (y != 1)
                                    {
                                        if (list.Count > 0)
                                        {
                                            this.RunOnUiThread(() =>
                                            {
                                                cs.ScanerInit.ActiveScaner(false);
                                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                    .SetCancelable(false)
                                                    .SetMessage(/*"Не удалось обновить информацию о товаре со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message35) + " " +
                                                                CurrentBarcode + " " +/*"в текущем документе"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message32))
                                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                        (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                    .Show();
                                            });
                                            return;
                                        }
                                        else
                                        {
                                            this.RunOnUiThread(() =>
                                            {
                                                cs.ScanerInit.ActiveScaner(false);
                                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                    .SetMessage(/*"Не удалось добавить товар со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message40) + " " + CurrentBarcode +
                                                                " " +/*"в текущий документ"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message41))
                                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                    .SetCancelable(false)
                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                        (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                    .Show();
                                            });
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if (list.Count > 0)
                                        {
                                            using (cs.ScanHistory history = new cs.ScanHistory())
                                            {
                                                double countAddN = 0;
                                                string barcName;
                                                if (statusBarcodeModel.StatusProduct)
                                                {
                                                    countAddN = Convert.ToDouble(statusBarcodeModel.WeightProduct);
                                                    barcName = barcode;
                                                }
                                                else
                                                {
                                                    //добавил блок 15.09.2025
                                                    //02.10.2025_#2 заменил  UsedAccountingParties на UsedAccountingParties_NEW
                                                    if (cs.Settings.AccountingPartiesClass.UsedAccountingParties_NEW() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                    {
                                                        countAddN = statusBarcodeModel.compositeBarcodeModel.count;
                                                        barcName = barcodes[0].BarcodeName;
                                                    }
                                                    else
                                                    {
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                        countAddN = barcodes[0].Count;
                                                        barcName = barcodes[0].BarcodeName;
                                                        //добавил блок 15.09.2025
                                                    }
                                                    //КОНЕЦ добавил блок 15.09.2025
                                                }

                                                //01.09.2025 заменил имя метода First на First_Ext
                                                var tre = /*await*/ history.AddHistory/*Async*/(this, tyuio.First_Ext(), countAddN, barcName, 0);
                                                if (tre == false)
                                                {
                                                    this.RunOnUiThread(() =>
                                                    {
                                                        cs.ScanerInit.ActiveScaner(false);
                                                        cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                            .SetMessage(
                                                                /*"Не удалось обновить историю сканирований товара со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message37) + " " +
                                                                barcodes[0].BarcodeName + " " +/*"для текущего документа"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message36))
                                                            .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                            .SetCancelable(false)
                                                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                (senderAlert, args) =>
                                                                {
                                                                    cs.ScanerInit.ActiveScaner(true);
                                                                })
                                                            .Show();
                                                    });
                                                }
                                                else
                                                {
                                                    adapter.current.ScanHistory.Add(cs.ScanHistory.history);
                                                    adapter.current.Count_Real += countAddN;
                                                    adapter.Update();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //добавил 07.02.2024
                                            if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                            {
                                                //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                                //не сохранять историю
                                            }
                                            else
                                            {
                                                //добавил блок 19.06.2025
                                                if (cs.BaseClass.currentDocHead.DocType == 1 && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuIncomingCheckBox", false))
                                                {
                                                    //не сохранять историю
                                                }
                                                else
                                                {
                                                    //КОНЕЦ добавил блок 19.06.2025
                                                    using (cs.ScanHistory history = new cs.ScanHistory())
                                                    {
                                                        double countAddN = 0;
                                                        string barcName;
                                                        if (statusBarcodeModel.StatusProduct)
                                                        {
                                                            countAddN = Convert.ToDouble(statusBarcodeModel.WeightProduct);
                                                            barcName = barcode;
                                                        }
                                                        else
                                                        {
                                                            //добавил блок 15.09.2025
                                                            //02.10.2025_#2 заменил  UsedAccountingParties на UsedAccountingParties_NEW
                                                            if (cs.Settings.AccountingPartiesClass.UsedAccountingParties_NEW() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                            {
                                                                countAddN = statusBarcodeModel.compositeBarcodeModel.count;
                                                                barcName = barcodes[0].BarcodeName;
                                                            }
                                                            else
                                                            {
                                                                //КОНЕЦ добавил блок 15.09.2025
                                                                countAddN = barcodes[0].Count;
                                                                barcName = barcodes[0].BarcodeName;
                                                                //добавил блок 15.09.2025
                                                            }
                                                            //КОНЕЦ добавил блок 15.09.2025
                                                        }

                                                        var tre = /*await*/ history.AddHistory/*Async*/(this, adapter.current, countAddN, barcName, 0);
                                                        if (tre == false)
                                                        {
                                                            this.RunOnUiThread(() =>
                                                            {
                                                                cs.ScanerInit.ActiveScaner(false);
                                                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");

                                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                    .SetMessage(
                                                                        /*"Не удалось обновить историю сканирований товара со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message37) + " " +
                                                                        barcodes[0].BarcodeName + " " +/*"для текущего документа"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message36))
                                                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                                    .SetCancelable(false)
                                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                                        (senderAlert, args) =>
                                                                        {
                                                                            cs.ScanerInit.ActiveScaner(true);
                                                                        })
                                                                    .Show();
                                                            });
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //добавил блок 02.02.2026
                                    cs.Logs.TimeMeasureLoggerAsync.Instance.Stop(adapter?.current?.GoodF, barcode);
                                    //КОНЕЦ добавил блок 02.02.2026
                                }
                                catch (System.Exception ex)
                                {
                                    ex = ex;
                                    this.RunOnUiThread(() =>
                                    {
                                        cs.ScanerInit.ActiveScaner(false);
                                        cs.Sound.NotificationSound.PlayNotification(this, "invalid");

                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                            .SetMessage(/*"Не удалось обновить/добавить информацию о товаре со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message44) + " " +
                                                        CurrentBarcode + " " +/*"в текущем документе"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message32))
                                            .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                            .SetCancelable(false)
                                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                            .Show();
                                    });
                                    return;
                                }
                            }

                            #endregion

                            #region если выключен режим автосканирования

                            else
                            {
                                bool isOpenedProductDialog = false;
                                //if (productDialog != null)
                                //{
                                //    try
                                //    {
                                //        isOpenedProductDialog = productDialog.isOpened;
                                //    }
                                //    catch (System.Exception)
                                //    {

                                //    }
                                //}
                                if (!isOpenedProductDialog)
                                {
                                    try
                                    {
                                        cs.DocDetails.CardProductDialog.Barcode = barcode;
                                        cs.DocDetails.CardProductDialog.editFactCardProduct = editFactCardProduct;
                                        cs.DocDetails.CardProductDialog.ExsistInDocu = true;
                                        cs.DocDetails.CardProductDialog.currentGood = goods[0];
                                        if (!statusBarcodeModel.StatusProduct)
                                        {
                                            cs.DocDetails.CardProductDialog.currentBarcode = barcodes[0];
                                        }

                                        cs.DocDetails.CardProductDialog.TypeDocu = cs.BaseClass.currentDocHead.DocType;
                                        //04.04.2025 закоментировал cs.DocDetails.CardProductDialog.StatusBarcodeModel
                                        //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                        /* cs.DocDetails.CardProductDialog.StatusBarcodeModel*/
                                        cs.BaseClass.statusBarcodeModel = statusBarcodeModel;
                                        OpenCardProduct(adapter.current, adapter);
                                    }
                                    catch (System.Exception ex2)
                                    {
                                        ex2 = ex2;
                                        using (GenerateLogException generateLog = new GenerateLogException())
                                        {
                                            generateLog.InfoException(ex2, "ListDocDetailsActivity.barcodeScanOperation_1", cs.BaseClass.FreeMemoryMessage());
                                            cs.ScanerInit.ActiveScaner(false);
                                            this.RunOnUiThread(() =>
                                            {
                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                    .SetMessage(/*"Ошибка"*/Android.App.Application.Context.GetText(Resource.String.Error) + ": " + ex2.GetBaseException().Message)
                                                    .SetTitle(/*"Ошибка"*/Android.App.Application.Context.GetText(Resource.String.Error))
                                                    .SetCancelable(false)
                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK), (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                    .Show();
                                            });
                                            return;
                                        }
                                    }

                                }
                            }

                            #endregion

                            #endregion
                        }
                        //если не найден в документе
                        else
                        {
                            //добавил блок 21.11.2024
                            if (adapter != null)
                            {
                                if (adapter?.current != null)
                                {
                                    adapter.current = null;
                                }
                            }
                            //КОНЕЦ добавил блок 21.11.2024

                            #region не найден в документе

                            //добавил блок 09.12.2024_#3
                            //добавил в условие "!FindedProduct" 11.12.2024_#3
                            if (!FindedProduct && PlanOrNewDocu && cs.BaseClass.currentDocHead.DocType == 4 && !cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false) && !cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventment_NotNewProductCheckBox", true))
                            {
                                //добавил 09.12.2024 (для документа перемещения с включенным ячеечным хранением и с неактивным статусом перемещения "StateMovedFrom==false"
                                //и с запретом добавления новых товаров в плановый документ)
                                cs.ScanerInit.ActiveScaner(false);
                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                    .SetMessage(/*Товар не найден в документе*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message11))
                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                    .SetCancelable(false)
                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                        (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                    .Show();
                                return;
                            }
                            //КОНЕЦ добавил блок 09.12.2024_#3

                            //23.01.2026_#2 добавил блок 
                            if (Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.IsWaveAssemblyDocument())
                            {
                                //добавил 09.12.2024 (для документа Волновая сборка
                                //и с запретом добавления новых товаров в плановый документ)
                                cs.ScanerInit.ActiveScaner(false);
                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                    .SetMessage(/*Товар не найден в задании*/Android.App.Application.Context.GetText(Resource.String.WaveAssembly_Message25))
                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                    .SetCancelable(false)
                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                        (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                    .Show();
                                return;
                            }
                            //КОНЕЦ 23.01.2026_#2 добавил блок

                            //добавил 07.02.2024
                            if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                            {
                                //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                cs.ScanerInit.ActiveScaner(false);
                                cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                    .SetMessage(/*Товар не найден в документе*/Android.App.Application.Context.GetText(Resource.String.cs_Docs_CardProductDialogDocu_Message11))
                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                    .SetCancelable(false)
                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                        (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                    .Show();
                                return;
                            }
                            else
                            {
                                #region при работе с ячейками в плановых документах с развернутым списком и с включенным строгим режимом

                                if (cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) &&
                                cs.BaseClass.preferences.GetBoolean("StrongCellCheckBox", true) /*&&
                            SmartStoreData.SourceDataBase.showDocDetails*/ && PlanOrNewDocu)
                                {
                                    cs.ScanerInit.ActiveScaner(false);
                                    cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                    new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                        .SetMessage(/*"В документе отсутствует товар, который расположен в текущей ячейке"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message28))
                                        .SetTitle(/*"Товар"*/Android.App.Application.Context.GetText(Resource.String.Product))
                                        .SetCancelable(false)
                                        //.SetIcon(Resource.Drawable.SS_icon2)
                                        .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK), (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                        .Show();
                                    return;

                                }

                                #endregion

                                bool newpos = false;

                                //если новый (слепой) документ, тогда разрешаем добавлять новые позиции, в не зависимости от того, что стоит в настройках
                                if (!ListDocDetailsActivity.PlanOrNewDocu)
                                {
                                    newpos = true;
                                }
                                else
                                {
                                    #region читаю с настроек, можно ли добавлять новые позиции в документ

                                    switch (cs.BaseClass.currentDocHead.DocType)
                                    {
                                        case 1:
                                            newpos = cs.BaseClass.preferences.GetBoolean("InCommingDocCheckBox", true);
                                            break;
                                        case 2:
                                            newpos = cs.BaseClass.preferences.GetBoolean("OutCommingDocCheckBox", true);
                                            break;
                                        case 3:
                                            newpos = cs.BaseClass.preferences.GetBoolean("InventoryDocCheckBox", true);
                                            break;
                                        case 4:
                                            //добавил 07.02.2024
                                            if (cs.BaseClass.currentDocHead.DocType == 4 && cs.BaseClass.currentDocHead.StateMovedFrom && cs.BaseClass.preferences.GetBoolean("CellCheckBox", false) && cs.BaseClass.preferences.GetBoolean("SpecialDocuMoventmentCheckBox", false))
                                            {
                                                //добавил 07.02.2024 (для документа перемещения с включенным ячеечным хранением и с активным статусом перемещения "StateMovedFrom==true")
                                                newpos = false;
                                            }
                                            else
                                            {
                                                newpos = cs.BaseClass.preferences.GetBoolean("MovingDocCheckBox", true);
                                            }
                                            //newpos = cs.BaseClass.preferences.GetBoolean("MovingDocCheckBox", true);
                                            break;
                                        case 5:
                                            newpos = cs.BaseClass.preferences.GetBoolean("WriteOffDocCheckBox", true);
                                            break;
                                        case 6:
                                            newpos = cs.BaseClass.preferences.GetBoolean("ReturnDocCheckBox", true);
                                            break;
                                        //добавил блок 26.08.2025
                                        case 9:
                                            newpos = cs.BaseClass.preferences.GetBoolean("ProductionDocCheckBox", true);
                                            break;
                                        //КОНЕЦ добавил блок 26.08.2025
                                        //добавил блок 03.01.2026
                                        case 10:
                                            newpos = cs.BaseClass.preferences.GetBoolean("WaveDocCheckBox", false);
                                            break;
                                            //КОНЕЦ добавил блок 03.01.2026
                                    }

                                    #endregion
                                }

                                //если можно, добавляю
                                if (newpos)
                                {
                                    //читаю с настроек, режим автосканирования
                                    bool modeautoscan = cs.BaseClass.preferences.GetBoolean("ModeAutoScanCheckBox", false);

                                    if (ScanGS1_128)
                                    {
                                        if (modeautoscan)
                                        {
                                            cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                            modeautoscan = false;
                                        }
                                    }
                                    //добавил блок 08.09.2025_#1
                                    else
                                    {
                                        if (cs.BaseClass.statusBarcodeModel.StatusProduct)
                                        {
                                            modeautoscan = false;
                                        }
                                    }
                                    //КОНЕЦ добавил блок 08.09.2025_#1

                                    //добавил блок 21.05.2024
                                    //запрещаю режим автосканирования
                                    if (cs.BaseClass.preferences.GetBoolean("ForTypesDocuments_Incoming_ExpirationDate", false) && cs.BaseClass.currentDocHead.DocType == 1 && goods[0].Field_2 == ExpirationDateDialog.MarkerExpirationDate)
                                    {
                                        if (modeautoscan)
                                        {
                                            //cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                            modeautoscan = false;
                                        }
                                    }
                                    //КОНЕЦ добавил блок 21.05.2024

                                    //добавил блок 15.09.2025
                                    //02.10.2025_#2 заменил  UsedAccountingParties на UsedAccountingParties_NEW
                                    if (cs.Settings.AccountingPartiesClass.UsedAccountingParties_NEW() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                    {
                                        if (modeautoscan)
                                        {
                                            //cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                            modeautoscan = false;
                                        }
                                    }
                                    //КОНЕЦ добавил блок 15.09.2025

                                    //добавил блок 21.01.2026_#1
                                    if (Layers.Documents.WaveAssembly.WaveAssemblyClass.Instance.IsWaveAssemblyDocument())
                                    {
                                        if (modeautoscan)
                                        {
                                            modeautoscan = false;
                                        }
                                    }
                                    //КОНЕЦ добавил блок 21.01.2026_#1

                                    #region если включен режим автосканирования и editFactCardProduct==false
                                    if (modeautoscan && !editFactCardProduct)
                                    {
                                        //добавил блок 04.08.2025_#1
                                        if (barcodes == null)
                                        {
                                            //cs.ScanerInit.ActiveScaner(false);
                                            //cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                            //new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                            //    .SetMessage("Режим автосканирования будет отключен")
                                            //    .SetTitle("Сканер")
                                            //    .SetCancelable(false)
                                            //    .SetPositiveButton("OK",
                                            //        (senderAlert, args) => 
                                            //        {
                                            //            cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                            //            cs.ScanerInit.ActiveScaner(true);
                                            //        })
                                            //    .Show();
                                            //return;
                                            cs.Toasts.aToast.ShowToast(this,/* "Режим автосканирования был отключен"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message49),
                                                ToastLength.Long, true);
                                            //cs.AlerrtDialogs.AlertDialogs.AlertDialog_V7(this, "Режим автосканирования был отключен", "Сканер", false);
                                            cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                            barcodeScanOperation(dataBase, goods, barcodes, barcode, statusBarcodeModel);
                                            return;
                                        }
                                        else
                                        {
                                            //КОНЕЦ добавил блок 04.08.2025_#1
                                            if (barcodes[0].Count == 0)
                                            {
                                                //cs.ScanerInit.ActiveScaner(false);
                                                //cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                                //new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                //    .SetMessage("Режим автосканирования будет отключен")
                                                //    .SetTitle("Сканер")
                                                //    .SetCancelable(false)
                                                //    .SetPositiveButton("OK",
                                                //        (senderAlert, args) => 
                                                //        {
                                                //            cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                                //            cs.ScanerInit.ActiveScaner(true);
                                                //        })
                                                //    .Show();
                                                //return;
                                                cs.Toasts.aToast.ShowToast(this,/* "Режим автосканирования был отключен"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message49),
                                                    ToastLength.Long, true);
                                                //cs.AlerrtDialogs.AlertDialogs.AlertDialog_V7(this, "Режим автосканирования был отключен", "Сканер", false);
                                                cs.BaseClass.preferences.Edit().PutBoolean("ModeAutoScanCheckBox", false).Apply();
                                                barcodeScanOperation(dataBase, goods, barcodes, barcode, statusBarcodeModel);
                                                return;
                                            }
                                            //добавил блок 04.08.2025_#1
                                        }
                                        //КОНЕЦ добавил блок 04.08.2025_#1
                                        adapter.current = new SmartStoreData.DocDetails();

                                        adapter.current.Bad_price = false;
                                        if (cs.BaseClass.currentIdCell > 0)
                                        {
                                            adapter.current.CellF = cs.BaseClass.currentIdCell;
                                            if (SmartStoreData.SourceDataBase.showDocDetails)
                                            {
                                                adapter.current.Cell = dataBase.dataContext.Cell/*.AsNoTracking()*/
                                                    //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                                    .Where(c => c.CellF == adapter.current.CellF).FirstOrDefault_Ext();

                                                if (adapter.current.Cell != null)
                                                {
                                                    if (adapter.current.cells == null)
                                                    {
                                                        adapter.current.cells = new List<Cell>();
                                                    }

                                                    adapter.current.cells.Add(adapter.current.Cell);
                                                }
                                            }
                                            else
                                            {
                                                adapter.current.Cell = dataBase.dataContext.Cell/*.AsNoTracking()*/
                                                    //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                                    .Where(c => c.CellF == adapter.current.CellF).FirstOrDefault_Ext();

                                                if (adapter.current.Cell != null)
                                                {
                                                    if (adapter.current.cells == null)
                                                    {
                                                        adapter.current.cells = new List<Cell>();
                                                    }

                                                    adapter.current.cells.Add(adapter.current.Cell);
                                                }
                                            }
                                        }

                                        //флаг ручного изменения истории
                                        adapter.current.Change_history = false;
                                        adapter.current.Count_Doc = 0;
                                        if (statusBarcodeModel.StatusProduct)
                                        {
                                            adapter.current.Count_Real =
                                                System.Math.Round(Convert.ToDouble(statusBarcodeModel.WeightProduct), 3);
                                        }
                                        else
                                        {
                                            adapter.current.Count_Real = barcodes[0].Count;
                                        }

                                        adapter.current.CreateDate = DateTime.Now.Ticks;
                                        adapter.current.DocHeadF = cs.BaseClass.currentDocHead.DocHeadF;
                                        if (statusBarcodeModel.StatusProduct)
                                        {
                                            try
                                            {
                                                //01.09.2025 заменил имя метода First на First_Ext
                                                adapter.current.GoodF = goods.First_Ext().GoodF;
                                            }
                                            catch
                                            {
                                                adapter.current.GoodF = statusBarcodeModel.CodeProduct;
                                            }
                                        }
                                        else
                                        {
                                            adapter.current.GoodF = goods[0].GoodF;
                                        }

                                        ////////adapter.current.Have_comment = false;
                                        ////////adapter.current.Have_spec_comment = false;
                                        //ручной ввод
                                        adapter.current.Hend_enter = cs.BaseClass.HendEnter;
                                        adapter.current.DocDetailsF = Guid.NewGuid();

                                        #region подгружаем цену для указанного склада

                                        List<PriceAndRemains> t = new List<PriceAndRemains>();
                                        try
                                        {
                                            if (cs.BaseClass.currentDocHead.MainStoreF != null)
                                            {

                                                switch (cs.BaseClass.preferences.GetString("TypeDataBase", "1"))
                                                {
                                                    case "1":
                                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                        t = StoresProducts.Where(c =>
                                                  c.GoodF == adapter.current.GoodF).ToList_Ext();
                                                        break;
                                                    case "2":
                                                        //01.09.2025 заменил имя метода ToList на ToList_Ext
                                                        t = dataBase.dataContext.PriceAndRemains.AsNoTracking().Where(c =>
                                                   c.GoodF == adapter.current.GoodF &&
                                                   c.StoreF == cs.BaseClass.currentDocHead.MainStoreF)
                                               .ToList_Ext();
                                                        break;
                                                }
                                            }
                                        }
                                        catch
                                        {
                                        }

                                        if (t.Count == 1)
                                        {
                                            //////adapter.current.Price = t[0].Price;
                                            if (adapter.current.Good != null)
                                            {
                                                adapter.current.Good.Price = t[0].Price;
                                            }
                                        }
                                        else
                                        {
                                            ////// adapter.current.Price = goods[0].Price;
                                            if (adapter.current.Good != null)
                                            {
                                                adapter.current.Good.Price = goods[0].Price;
                                            }
                                        }

                                        #endregion

                                        ////////adapter.current.SummDoc = null;
                                        ////////adapter.current.SummReal = null;
                                        adapter.current.UpdatedFromTSD = true;
                                        adapter.current.UpdateFrom1C = false;
                                        adapter.current.UserF = cs.BaseClass.currentIdUser;
                                        if (SmartStoreData.SourceDataBase.LocalDB == SmartStoreData.TypeDatabaseEnum.local)
                                        {
                                            adapter.current.id = 1;
                                            try
                                            {
                                                LinqExtensions.WriteErrorToFile = false;
                                                //29.08.2025 заменил .Max на .Max_Ext
                                                adapter.current.id = dataBase.dataContext.DocDetails.AsNoTracking().Max_Ext(c => c.id);
                                                LinqExtensions.WriteErrorToFile = true;
                                            }
                                            catch
                                            {
                                            }
                                        }
                                        var curentCell = adapter.current.Cell;
                                        var curentCells = adapter.current.cells;
                                        adapter.current.cells = null;
                                        adapter.current.Cell = null;

                                        //добавил блок 15.09.2025
                                        //02.10.2025_#2 закоментировал блок
                                        //if (cs.Settings.AccountingPartiesClass.UsedAccountingParties() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                        //{
                                        //    adapter.current.BatchNumber = statusBarcodeModel.compositeBarcodeModel.batch_number;
                                        //    adapter.current.ViewPack = cs.Settings.AccountingPartiesClass.ConvertViewPack(statusBarcodeModel.compositeBarcodeModel.view_pack);
                                        //}
                                        //КОНЕЦ 02.10.2025_#2 закоментировал блок
                                        //КОНЕЦ добавил блок 15.09.2025
                                        dataBase.dataContext.DocDetails.Add(adapter.current);
                                        try
                                        {
                                            int y = dataBase.dataContext.SaveChanges();
                                            adapter.current.cells = curentCells;
                                            adapter.current.Cell = curentCell;
                                            //добавил блок 14.02.2024
                                            bool fbscan = false;
                                            if (barcodes != null && barcode != null)
                                            {
                                                if (barcodes.Count > 0)
                                                {
                                                    //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                                    var currentBarcodeScan = barcodes.Where(c => c.BarcodeName == barcode).FirstOrDefault_Ext();
                                                    if (currentBarcodeScan != null)
                                                    {
                                                        adapter.current.Good?.Barcode.Add(currentBarcodeScan);
                                                        fbscan = true;
                                                    }
                                                }
                                            }
                                            if (!fbscan && barcode != null)
                                            {
                                                //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                                var currentBarcodeScan = dataBase.dataContext.Barcode.Where(c => c.BarcodeName == barcode).FirstOrDefault_Ext();
                                                if (currentBarcodeScan != null)
                                                {
                                                    adapter.current.Good?.Barcode.Add(currentBarcodeScan);
                                                }
                                            }
                                            //КОНЕЦ добавил блок 14.02.2024
                                            if (y != 1)
                                            {
                                                this.RunOnUiThread(() =>
                                                {
                                                    new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                        .SetMessage(/*"Не удалось добавить товар со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message40) + " " + CurrentBarcode +
                                                                    " " +/*"в текущий документ"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message41))
                                                        .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                        .SetCancelable(false)
                                                        .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK), (senderAlert, args) => { })
                                                        .Show();
                                                });
                                                return;
                                            }
                                            else
                                            {

                                                using (cs.ScanHistory history = new cs.ScanHistory())
                                                {
                                                    double countAddN = 0;
                                                    string barcName;
                                                    if (statusBarcodeModel.StatusProduct)
                                                    {
                                                        countAddN = Convert.ToDouble(statusBarcodeModel.WeightProduct);
                                                        barcName = barcode;
                                                    }
                                                    else
                                                    {
                                                        //добавил блок 15.09.2025
                                                        //02.10.2025_#2 заменил  UsedAccountingParties на UsedAccountingParties_NEW
                                                        if (cs.Settings.AccountingPartiesClass.UsedAccountingParties_NEW() && cs.Settings.AccountingPartiesClass.isAccountingPartiesBarcode())
                                                        {
                                                            countAddN = statusBarcodeModel.compositeBarcodeModel.count;
                                                            barcName = barcodes[0].BarcodeName;
                                                        }
                                                        else
                                                        {
                                                            //КОНЕЦ добавил блок 15.09.2025
                                                            countAddN = barcodes[0].Count;
                                                            barcName = barcodes[0].BarcodeName;
                                                            //добавил блок 15.09.2025
                                                        }
                                                        //КОНЕЦ добавил блок 15.09.2025
                                                    }

                                                    var tre = /*await*/ history.AddHistory/*Async*/(this, adapter.current, countAddN, barcName, 0);

                                                    if (tre == false)
                                                    {
                                                        this.RunOnUiThread(() =>
                                                        {
                                                            new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                                .SetMessage(
                                                                    /*"Не удалось обновить историю сканирований товара со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message37) + " " +
                                                                    barcodes[0].BarcodeName + " " +/*"для текущего документа"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message36))
                                                                .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                                .SetCancelable(false)
                                                                .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK), (senderAlert, args) => { })
                                                                .Show();
                                                        });
                                                    }
                                                }
                                            }
                                            //добавил блок 02.02.2026
                                            cs.Logs.TimeMeasureLoggerAsync.Instance.Stop(adapter?.current?.GoodF, barcode);
                                            //КОНЕЦ добавил блок 02.02.2026
                                        }
                                        catch (System.Exception ex)
                                        {
                                            ex = ex;
                                            this.RunOnUiThread(() =>
                                            {
                                                new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                    .SetMessage(/*"Не удалось добавить товар со штрихкодом"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message40) + " " + CurrentBarcode +
                                                                " " +/*"в текущий документ"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message41))
                                                    .SetTitle(/*"Документ"*/Android.App.Application.Context.GetText(Resource.String.DocumentTitle))
                                                    .SetCancelable(false)
                                                    .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK), (senderAlert, args) => { })
                                                    .Show();
                                            });
                                            return;
                                        }

                                        if (adapter.current.Good == null && barcode != null)
                                        {
                                            //01.09.2025 заменил имя метода First на First_Ext
                                            adapter.current.Good = dataBase.dataContext.Good.AsNoTracking().Include(c => c.Barcode).Where(c => c.GoodF == adapter.current.GoodF).First_Ext();
                                            //добавил блок 14.02.2024
                                            if (adapter.current.Good != null)
                                            {
                                                if (adapter.current.Good.Barcode.Count > 0)
                                                {
                                                    //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                                    var currentBarcodeScan = adapter.current.Good.Barcode.Where(c => c.BarcodeName == barcode).FirstOrDefault_Ext();
                                                    if (currentBarcodeScan != null)
                                                    {
                                                        adapter.current.Good?.Barcode?.Clear();
                                                        adapter.current.Good?.Barcode?.Add(currentBarcodeScan);
                                                    }
                                                }
                                                else
                                                {
                                                    //01.09.2025 заменил имя метода FirstOrDefault на FirstOrDefault_Ext
                                                    var currentBarcodeScan = barcodes.Where(c => c.BarcodeName == barcode).FirstOrDefault_Ext();
                                                    if (currentBarcodeScan != null)
                                                    {
                                                        adapter.current.Good?.Barcode?.Clear();
                                                        adapter.current.Good?.Barcode?.Add(currentBarcodeScan);
                                                    }
                                                }
                                            }
                                            //КОНЕЦ добавил блок 14.02.2024
                                        }

                                        #region повторно подгружаем цену для указанного склада

                                        if (t.Count == 1)
                                        {
                                            //////adapter.current.Price = t[0].Price;
                                            if (adapter.current.Good != null)
                                            {
                                                adapter.current.Good.Price = t[0].Price;
                                            }
                                        }

                                        #endregion

                                        //вставляю в начала списка товар
                                        adapter.Insert(adapter.current, 0);

                                        //ProductsListView.SetItemChecked(0, true);
                                    }

                                    #endregion

                                    #region если выключен режим автосканирования

                                    else
                                    {
                                        bool isOpenedProductDialog = false;
                                        //if (productDialog != null)
                                        //{
                                        //    try
                                        //    {
                                        //        isOpenedProductDialog = productDialog.isOpened;
                                        //    }
                                        //    catch (System.Exception)
                                        //    {

                                        //    }
                                        //}
                                        if (!isOpenedProductDialog)
                                        {
                                            try
                                            {
                                                cs.DocDetails.CardProductDialog.Barcode = barcode;
                                                cs.DocDetails.CardProductDialog.editFactCardProduct = editFactCardProduct;
                                                cs.DocDetails.CardProductDialog.ExsistInDocu = false;
                                                cs.DocDetails.CardProductDialog.currentGood = goods[0];
                                                if (statusBarcodeModel.StatusProduct == false)
                                                {
                                                    cs.DocDetails.CardProductDialog.currentBarcode = barcodes[0];
                                                }
                                                //04.04.2025 закоментировал cs.DocDetails.CardProductDialog.StatusBarcodeModel
                                                //04.04.2025 добавил cs.BaseClass.statusBarcodeModel
                                                /*cs.DocDetails.CardProductDialog.StatusBarcodeModel*/
                                                cs.BaseClass.statusBarcodeModel = statusBarcodeModel;
                                                cs.DocDetails.CardProductDialog.TypeDocu = cs.BaseClass.currentDocHead.DocType;
                                                OpenCardProduct(adapter.current, adapter);
                                            }
                                            catch (System.Exception ex2)
                                            {
                                                ex2 = ex2;
                                                using (GenerateLogException generateLog = new GenerateLogException())
                                                {
                                                    generateLog.InfoException(ex2, "ListDocDetailsActivity.barcodeScanOperation_3", cs.BaseClass.FreeMemoryMessage());
                                                    cs.ScanerInit.ActiveScaner(false);
                                                    this.RunOnUiThread(() =>
                                                    {
                                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                                            .SetMessage(/*"Ошибка"*/Android.App.Application.Context.GetText(Resource.String.Error) + ": " + ex2.GetBaseException().Message)
                                                            .SetTitle(/*"Ошибка"*/Android.App.Application.Context.GetText(Resource.String.Error))
                                                            .SetCancelable(false)
                                                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK), (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                                            .Show();
                                                    });
                                                    return;
                                                }
                                            }

                                        }


                                    }

                                    #endregion
                                }
                                //если нельзя
                                else
                                {
                                    this.RunOnUiThread(() =>
                                    {
                                        cs.ScanerInit.ActiveScaner(false);
                                        cs.Sound.NotificationSound.PlayNotification(this, "invalid");
                                        new AlertDialog.Builder(this, Resource.Style.DialogTheme)
                                            .SetMessage(/*"Запрещено добавлять новые позиции товаров в текущий документ"*/Android.App.Application.Context.GetText(Resource.String.cs_ListDocDetailsActivity_Message52))
                                            .SetTitle(/*"Разрешения"*/Android.App.Application.Context.GetText(Resource.String.cs_DocDetails_CardProductDialog_Message33))
                                            .SetCancelable(false)
                                            //.SetIcon(Resource.Drawable.SS_icon2)
                                            .SetPositiveButton(/*"OK"*/Android.App.Application.Context.GetText(Resource.String.OK),
                                                (senderAlert, args) => { cs.ScanerInit.ActiveScaner(true); })
                                            .Show();
                                    });

                                    return;
                                }
                            }
                            #endregion
                        }

                    }

                    #endregion
                }

                #endregion
            }
            catch (System.Exception ex)
            {
                using (GenerateLogException generateLog = new GenerateLogException())
                {
                    generateLog.InfoException(ex, "ListDocDetailsActivity.barcodeScanOperation#1", cs.BaseClass.FreeMemoryMessage());
                }
            }
        }

        #endregion
