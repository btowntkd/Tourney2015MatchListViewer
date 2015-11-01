using SpiritMVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchListViewer
{
    public class MatchItemVM : ObservableObject
    {
        private int? _matchID;
        private int? _ringID;
        private string _blueName;
        private int? _blueNextMatchID;
        private string _redName;
        private int? _redNextMatchID;

        public int? MatchID
        {
            get { return _matchID; }
            set { Set(ref _matchID, value); }
        }

        public int? RingID
        {
            get { return _ringID; }
            set { Set(ref _ringID, value); }
        }

        public string BlueName
        {
            get { return _blueName; }
            set { Set(ref _blueName, value); }
        }

        public int? BlueNextMatchID
        {
            get { return _blueNextMatchID; }
            set { Set(ref _blueNextMatchID, value); }
        }

        public string RedName
        {
            get { return _redName; }
            set { Set(ref _redName, value); }
        }

        public int? RedNextMatchID
        {
            get { return _redNextMatchID; }
            set { Set(ref _redNextMatchID, value); }
        }
    }
}
