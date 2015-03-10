
public class CharacterLookAtLocalPlayer : CharacterLookAt {
    protected override void Start()
    {
        base.Start();
        UseCharacterOffset();
        if (lookAtGameObject == null && UtopiaGameManager.Inst.LocalPlayer != null)
            lookAtGameObject = UtopiaGameManager.Inst.LocalPlayer.gameObject;
    }	
}
