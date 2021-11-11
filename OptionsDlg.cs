using System;
using System.Windows.Forms;

/*
 * Primary class defines the partial class of the options dialog for the
 * Kaboooom! game.
 *
 * Author:  M. G. Slack
 * Written: 2021-10-30
 *
 * ----------------------------------------------------------------------------
 * 
 * Updated: yyyy-mm-dd - xxxxx.
 *
 */
namespace Kaboooom
{
    public partial class OptionsDlg : Form
    {
        #region Properties
        private bool _safeGame = true;
        public bool SafeGame { get { return _safeGame; } set { _safeGame = value; } }

        private bool _soundsOn = false;
        public bool SoundsOn { get { return _soundsOn; } set { _soundsOn = value; } }

        private bool _verboseMove = false;
        public bool VerboseMove { get { return _verboseMove; } set { _verboseMove = value; } }

        private bool _randomBombs = true;
        public bool RandomBombs { get { return _randomBombs; } set { _randomBombs = value; } }

        private int _numberOfBombs = 20;
        public int NumberOfBombs { get { return _numberOfBombs; } set { _numberOfBombs = value; } }

        private bool _confirmations = false;
        public bool Confirmations { get { return _confirmations; } set { _confirmations = value; } }
        #endregion

        public OptionsDlg()
        {
            InitializeComponent();
        }

        #region Event Handlers
        private void OptionsDlg_Load(object sender, EventArgs e)
        {
            cbSafeGame.Checked = _safeGame;
            cbSounds.Checked = _soundsOn;
            cbVerbose.Checked = _verboseMove;
            cbRandomBombs.Checked = _randomBombs;
            nudMineCount.Value = _numberOfBombs;
            cbConfirm.Checked = _confirmations;
            if (_randomBombs)
                nudMineCount.Enabled = false;
            else
                nudMineCount.Enabled = true;
        }

        private void cbSafeGame_CheckedChanged(object sender, EventArgs e)
        {
            _safeGame = cbSafeGame.Checked;
        }

        private void cbSounds_CheckedChanged(object sender, EventArgs e)
        {
            _soundsOn = cbSounds.Checked;
        }

        private void cbVerbose_CheckedChanged(object sender, EventArgs e)
        {
            _verboseMove = cbVerbose.Checked;
        }

        private void cbRandomBombs_CheckedChanged(object sender, EventArgs e)
        {
            _randomBombs = cbRandomBombs.Checked;
            if (_randomBombs)
                nudMineCount.Enabled = false;
            else
                nudMineCount.Enabled = true;
        }

        private void nudMineCount_ValueChanged(object sender, EventArgs e)
        {
            _numberOfBombs = (int)nudMineCount.Value;
        }

        private void cbConfirm_CheckedChanged(object sender, EventArgs e)
        {
            _confirmations = cbConfirm.Checked;
        }
        #endregion
    }
}
