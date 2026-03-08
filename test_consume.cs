var mainManager = GameObject.Find("MainManager");
var magicM = mainManager.GetComponent<MagicManager>();
var btnGo = GameObject.Find("Btn_Research_UnlockBGM1");
var btnWrapper = btnGo.GetComponent<MagicButtonWrapper>();
btnWrapper.magicManager = magicM;
SaveManager.Current.player.inventory.money = 2000;
SaveManager.Current.player.inventory.AddMaterial("MAT_THREAD", 10);
btnWrapper.OnClick();
Debug.Log("ULOOP_TEST_MONEY_AFTER: " + SaveManager.Current.player.inventory.money);
