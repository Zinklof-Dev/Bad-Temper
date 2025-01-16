using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Mathematics;

public class LoadingManager : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject _LoadingCanvas;
    [SerializeField] TextMeshProUGUI _LoadingText;
    [SerializeField] TextMeshProUGUI _TipText;
    [SerializeField] RectTransform _LoadingSlider;
    [Header("Script References")]
    [SerializeField] TreeGeneration _TreeGen;
    [SerializeField] TerrainGeneration _TerrainGen;
    [Header("Player Spawn Points")]
    // To find circle of all points a distance from (0,0), do x^2 + y^2 = a^2
    [SerializeField] private Vector3[] spawnPoints;

    [Header("DebugVars")]
    [SerializeField] private float currentBarValue;
    [SerializeField] private float wantedBarValue;
    private int totalSteps = 5;
    private int stepsComplete;

    [SerializeField] private float timeElapsed;
    [SerializeField] private float minTimeElapsed;
    private float nextSnapCheck;
    private float nextHintChange;

    private bool _ServerHasSeed = false;
    private bool _CampfirePlaced = false;
    private int _Seed;

    private string[] loadingTips = {
    "This is a loading tip!",
    "Need a Dispenser here!",
    "Don't forget to breathe. You'd be shocked how much that helps... with anything really...",
    "Life lesson: Sometimes you're the hero, sometimes you're the NPC, you know which one you are, accept your fate, go make someones day.",
    "If you press every button at once, something magical might happen. Or you might just break your keyboard... same difference.",
    "You can't unsee your mistakes, <i>buuuut</i> you can blame them on someone else. Thats <i>totally</i> a good idea right?",
    "No one really reads these tips, but we wrote them anyways because... uh... I dunno.",
    "Fun fact, did you know dementia is... Fun fact, did you know dementia is... Fun fact, did you know dementia is... Fun fact, did you know dementia is... Fun fact, did you know dementia is...",
    "Hey, ever thought of why we exist? Have fun with that existential crisis :D.",
    "Can't beat that boss battle huh? Yeah just keep trying till you loose your dignity; this is the way.",
    "The real treasure is the friends we made along the way, but uh... you don't have those do you? Thats okay, no friends is better than fake friends.",
    "In life you're either the hero, or the background character that sits way off in the corner of shots randomly, guess which one you are? that's right, the main character of <b>your</b> story :)",
    "Remember, every video game hero starts off with nothing, don't be shamed that you've had it rough until here.",
    "Remember, the grind is just as important as the win, take a deep breath, and keep at it... I'm not talking about video games :).",
    "Never be afraid to ask for help, even the best players need to check a wiki sometimes, your friends care about you!",
    "When life gets tough, its an opportunity to level up, and not just video game characters level up.",
    "Celibrate every morning, maybe it's not a boss battle, but the little things matter... not just talking about the game.",
    "It's never too late to start something new. Whether its a hobby, a skill, a game, the first step is the hardest--but its worth it.",
    "Some nights are harder than others, that's okay, you get through them right? progress isn't linear, but every step counts, not talking about the game.",
    "It's okay to not be okay, sometimes the most courage lies in going onward when the world seems against you, be the hero of your story.",
    "You don't need to fight alone, bring a friend, or two, or more. It's okay to ask for some help, this isn't about the game.",
    "Healing isn't a race, be patient, every little bit matters.",
    "Real Talk: Feeling stuck, doesn't mean you're failing, shift your perspective, maybe shift that path a little, never give up",
    "The only bad pizza is the one you didn't eat.",
    "Pizza isn't just food, it's a reminder that good things come in all shapes, sizes, colors, crusts, peperoini... man now im hungry!",
    "Life is like pizza, sometimes its messy, sometimes it makes you feel awful, sometimes its just right, sometimes its a little too hard, but it's always worth it.",
    "Some days, the best thing you can do is get out of bed, bonus points if you put on pants!",
    "You'll never know what you're capable of until you try... or procrastinate for six hours and do it last second in a panic, same thing",
    "Trust me, you don't want things to be perfect, or to be perfect... You dont want to be remotely close to perfect, imagine the pressure! Be yourself, love your flaws! :D",
    "90% of life is showing up, 1% is knowing what you're doing, and 9% is looking like you know what you're doing, no one has it all together, keep your head up.",
    "Even a broken clock is right twice a day, keep trying, eventually you'll stumble on that right path, or the coffee machine, both are the same.",
    "If you don't know where you're going, don't worry, keep moving forward, soon enough you'll end up somewhere interesting, or with snacks, those keep me going.",
    "Life isn't a race, unless you're literally racing, then by all means make it a competition, but remember to enjoy the ride while you're at it.",
    "Its okay to have a bad day, just don't unpack your bags and live there, you're allowed to visit, not move in.",
    "Confidence is key. Just act like you know what you're doing, and people will assume you do, and soon enough you will. Trial by fire!",
    "Don't let life's small setbacks make you feel like you've failed, just like a game, you have plenty of time to try again!",
    "What?",
    "HuH?",
    "I made most of these loading screen tips motivational, because I, Zinklof, suffer with depression. I know what it's like to hurt, to feel alone. I want to remind you that you're not alone, life gets better, keep truckin onward, you'll find the light at that end of the tunnel, and be a cowpoke with a whole lotta stories to tell. And do tell them, let others know they aren't alone, and allow the trauma of the past to flow out of you, thats the best way to heal... alongside time of course. Have a blessed day :D."
    };

    public override void OnNetworkSpawn()
    {
        PreLoadChecklist();

        if (IsServer)
        {
            _Seed = UnityEngine.Random.Range(0, 99999);
            _ServerHasSeed = true;
            StartWorldGeneration();
        }
        else
        {
            AskForSeedRpc();
        }

        base.OnNetworkSpawn();
    }

    private void PreLoadChecklist()
    {
        minTimeElapsed = UnityEngine.Random.Range(28,32);
        timeElapsed = 0;
        nextHintChange = -1;
        nextSnapCheck = -1;
        _LoadingText.text = "Generating/Fetching Seed";
    }

    public void FinishStep(string nextStepText)
    {
        stepsComplete++;
        wantedBarValue = (float)stepsComplete/(float)totalSteps;
        _LoadingText.text = nextStepText;
    }

    private void EvaluateBar()
    {
        currentBarValue += (wantedBarValue - currentBarValue) * (1 * Time.deltaTime);

        if (timeElapsed > nextSnapCheck) // every 2 sec have chance to snap to next point
        {
            nextSnapCheck = timeElapsed + 2;
            int randomChance = UnityEngine.Random.Range(0, 101); // min exclusive, max exclusive, thus 0, 101 to get 0-100
            if (randomChance == 50) // one in 100 chance
            {
                currentBarValue = wantedBarValue;
            }
        }

        if (currentBarValue > 0.98f && wantedBarValue >= 1)
        {
            currentBarValue = 0.99f;
        }

        _LoadingSlider.sizeDelta = new Vector2(currentBarValue * 1300, 32);
    }

    private void ChangeLoadingTip()
    {
        if (timeElapsed > nextHintChange)
        {
            // assuming 180 wpm read speed (slow), thats 3 word per sec, average word in english is 4.7 chars long, so we will round up to 5, so for every 15 chars we give 3 sec, or for every 5 char we give 1 sec

            int index = UnityEngine.Random.Range(0, loadingTips.Length);
            string tip = loadingTips[index];

            float length = (tip.Length / 5); // results in 1 for every 5 chars, thus 1 every average word length, resulting in 1 sec per word.

            nextHintChange = timeElapsed + length;
            _TipText.text = tip;
        }
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        EvaluateBar();
        ChangeLoadingTip();

        if (wantedBarValue >= 0.99f && timeElapsed > minTimeElapsed)
        {
            _LoadingSlider = null;
            _LoadingText = null;
            Destroy(_LoadingCanvas);
            //code to teleport player, player script needs updated
            Destroy(this);
        }
    }

    private async void StartWorldGeneration()
    {
        FinishStep("Generating Terrain");
        await _TerrainGen.Initialize(_Seed);
        FinishStep("Initializing Campfire");

        if (IsServer)
        {
            await Campfire.Initialize(_Seed, gameObject);
            _CampfirePlaced = true;
        }
        FinishStep("Generating Trees/Rocks");

        await _TreeGen.Initialize(_Seed);
        FinishStep("Teleporting Player Object");
        AskToTeleportRpc();
        FinishStep("Awaiting Server...");
    }

    private async void AskAgain()
    {
        await Task.Delay(1000); // wait 500 ms, aka 0.5 secconds
        AskForSeedRpc(); // ask again
    }

    [Rpc(SendTo.Server)]
    private void AskForSeedRpc(RpcParams rpcParams = default)
    {
        ulong clientID = rpcParams.Receive.SenderClientId; // get client ID
        if (!_ServerHasSeed) // if the server doesn't yet have the seed then deny the clients request
        {
            DenySeedRequestRpc(RpcTarget.Single(clientID, RpcTargetUse.Temp));
            return;
        }
        else // otherwise provide the seed
            SendSeedRpc(_Seed, RpcTarget.Single(clientID, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SendSeedRpc(int seed, RpcParams rpcParams = default)
    {
        this._Seed = seed;
        StartWorldGeneration();
        
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void DenySeedRequestRpc(RpcParams rpcParams = default) // the server has denied our request. so lets wait and ask again
    {
        AskAgain();
    }

    [Rpc(SendTo.Server)]
    private void AskToTeleportRpc(RpcParams rpcParams = default)
    {
        ulong clientID = rpcParams.Receive.SenderClientId;
        if(!_CampfirePlaced)
        {
            DenyTeleportationRpc(RpcTarget.Single(clientID, RpcTargetUse.Temp));
            return;
        }
        else
        {
            Ray ray;
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(Campfire._position.x += spawnPoints[clientID].x, 9999, Campfire._position.z += spawnPoints[clientID].z), Vector3.down, out hit))
            {
                TeleportPlayerRpc(hit.point, RpcTarget.Single(clientID, RpcTargetUse.Temp));
            }
        }
    }
    [Rpc(SendTo.SpecifiedInParams)]
    private void DenyTeleportationRpc(RpcParams rpcParams = default)
    {
        AskToTeleportAgain();
    }
    [Rpc(SendTo.SpecifiedInParams)] 
    private void TeleportPlayerRpc(Vector3 position, RpcParams rpcParams = default)
    {
        Debug.Log("Player Position: " + position);
        Player.Teleport(position);
    }
    private async void AskToTeleportAgain()
    {
        await Task.Delay(1000);
        AskToTeleportRpc();
    }
}
