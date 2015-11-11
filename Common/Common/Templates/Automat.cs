using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Common
{
    public class Automat<T>
    {
        #region Делегады переходов

        /// <summary>
        /// Делегат выполняемый перед переходом из одного состояние в другое.
        /// </summary>
        /// <param name="oldSt">Состояние из которого происходит выход.</param>
        /// <param name="newSt">Состояние в которое происходит вход.</param>
        public delegate void OnChangingStateDelegate(T oldSt, T newSt, CancelEventArgs cancelEventArgs);

        /// <summary>
        /// Делегат выполняемый перед переходом в указанное состояние.
        /// </summary>
        /// <param name="state">Состояние в которое происходит вход.</param>
        public delegate void OnEntryDelegate(T state, CancelEventArgs cancelEventArgs);

        /// <summary>
        /// Делегат выполняемый после выхода из указанного состояния.
        /// </summary>
        /// <param name="state">Состояние из которого произходит выход.</param>
        public delegate void OnExitDelegate(T state);
        #endregion

        #region Коллекции событий переходов

        private T _State = default(T);
        List<Tuple<T, T, OnChangingStateDelegate>> _OnChangingStateDelegates = new List<Tuple<T, T, OnChangingStateDelegate>>();
        List<Tuple<T, OnEntryDelegate>> _OnEntryDelegates = new List<Tuple<T, OnEntryDelegate>>();
        List<Tuple<T, OnExitDelegate>> _OnExitDelegates = new List<Tuple<T, OnExitDelegate>>();

        #endregion

        /// <summary>
        /// Текущее состояние.
        /// </summary>
        public T State
        {
            get { return _State; }
            private set { _State = value; }
        }
        public bool ChangeStateIfNotRegistered { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initState"></param>
        /// <param name="changeStateIfNotRegistered">Все переходы по умолчанию разрешены,
        ///  если false то для возможности перехода необходимо его регистрировать методом AddChangingState.</param>
        public Automat(T initState = default(T), bool changeStateIfNotRegistered = true)
        {
            SetState(initState);
            ChangeStateIfNotRegistered = changeStateIfNotRegistered;
        }

        #region Методы

        /// <summary>
        /// Переход в указанное сосотояние.
        /// </summary>
        /// <param name="newState">Состояние в которое необходимо осуществить переход.</param>
        /// <returns>Показывает был ли осуществлен переход.</returns>
        public bool SetState(T newState)
        {
            T oldState = State;

            var сhangingStateDelegates = GetDelegates(_OnChangingStateDelegates, oldState, newState);
            var entryStateDelegates = GetDelegates(_OnEntryDelegates, newState);

            if (сhangingStateDelegates.Count() == 0 && !ChangeStateIfNotRegistered) return false;
            foreach (var item in сhangingStateDelegates)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                item(oldState, newState, cancelEventArgs);
                if (cancelEventArgs.Cancel) return false;
            }

            foreach (var item in entryStateDelegates)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                item(newState, cancelEventArgs);
                if (cancelEventArgs.Cancel) return false;
            }

            State = newState;
            GetDelegates(_OnExitDelegates, oldState).ToList().ForEach(d => d(oldState));

            return true;
        }

        #region Добавление событий переходов.

        /// <summary>
        /// Добавляет событие происходящее перед переходом из одного состояния в другое.
        /// </summary>
        /// <param name="oldST">Состояние из которого происходит выход.</param>
        /// <param name="newSt">Состояние в которое происходит вход.</param>
        /// <param name="changingStateDelegate">Метод выполняемый перед переходом.</param>
        /// <returns></returns>
        public Automat<T> AddChangingState(T oldST, T newSt, OnChangingStateDelegate changingStateDelegate)
        {
            _OnChangingStateDelegates.Add(new Tuple<T, T, OnChangingStateDelegate>(oldST, newSt, changingStateDelegate));
            return this;
        }

        /// <summary>
        /// Добавляет событие происходящее перед переходом в указанное состояние.
        /// </summary>
        /// <param name="state">Состояние в которое происходит переход.</param>
        /// <param name="onEntryDelegate">Метод выполняемый при входе в указанное состояние.</param>
        /// <returns></returns>
        public Automat<T> AddOnEntryState(T state, OnEntryDelegate onEntryDelegate)
        {
            _OnEntryDelegates.Add(new Tuple<T, OnEntryDelegate>(state, onEntryDelegate));
            return this;
        }

        /// <summary>
        /// Добавляет событие происходящее после выхода из указанного состояния.
        /// </summary>
        /// <param name="state">Состояние из которого происходит выход.</param>
        /// <param name="onExitDelegate">Метод выполняемый после перехода.</param>
        /// <returns></returns>
        public Automat<T> AddOnExitState(T state, OnExitDelegate onExitDelegate)
        {
            _OnExitDelegates.Add(new Tuple<T, OnExitDelegate>(state, onExitDelegate));
            return this;
        }
        #endregion

        private IEnumerable<DType> GetDelegates<DType>(List<Tuple<T, T, DType>> _ListDelegates, T oldState, T newState)
        {
            return _ListDelegates.Where(t => t.Item1.Equals(oldState) && t.Item2.Equals(newState)).Select(t => t.Item3);
        }

        private IEnumerable<DType> GetDelegates<DType>(List<Tuple<T, DType>> _ListDelegates, T state)
        {
            return _ListDelegates.Where(t => t.Item1.Equals(state)).Select(t => t.Item2);
        }
        #endregion


    }
}
