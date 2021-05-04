using UnityEngine;
using WebSocketSharp;
using System;
using System.Collections.Generic;
using System.Collections;

public class ConnectionTest : MonoBehaviour
{
    private SocketManager manager = SocketManager.GetSingleton();
    private long recieveTime, sendTime = 0;
    private bool stopSending = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TestSocket());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) stopSending = true;
    }

    IEnumerator TestSocket() 
    {
        manager.AddMessageEvent(
            (sender, e) => 
            {
                recieveTime = DateTime.Now.Ticks;
                print($"It took {(recieveTime - sendTime) / 10000} milliseconds\n To recieve \"{e.Data}\"");
            }
        );

        string words = 
"                                                                                                                                                                \n"+
"                     ``./oo:`                         `/o/:.                                                                                                    \n"+
"            smmhshhdmmMMMMMMm/                         +dMMMms-                                               ,ee,                                              \n"+
"            -hMMMMhooo+/oMMMM/                           /MMMMh                                               oNMmdhyyyy:                                       \n"+
"             `yMMMy     `MMMd`                            mMMMh                                                 ``----```                                       \n"+
"              `dMMN..--:+MMM+           .+ydmNNmdy:`      mMMMh                                         +ss+++++++osyhhmNNNh:                                   \n"+
"               :MMMmmNNNNNmms          :mMMNhoohNMMm/     dMMMh ,e,                                     `o:/`` ,/mmddhhy;,  `                                   \n"+
"                        ...,.,        `dMMM      mMMM-    hMMMNyhhddo   -sdNMMMMNdy/.       ::.              .mMMNy++smMMd.          -sdNMMMMNdy/.       ::.    \n"+
"    oddhhhhhdddmmmddmmNNMNmmmNNNN+     .yNMMmhhdNMMNo     yMMMh        /NMNho:/ohmMMNds//oymMMh`             +MMMs    :MMMo         /NMNho:/ohNMMNds//oymMMy    \n"+
"    `-ohmNNmdhs+/:--..sMMNh:..--.        -+hdmmmdy+.      yMMMh`.-..   `//-      .:sdmNMMNNmh+`              `hMMNyo+smMMm-         `//-      .:sdmNMMNNmh+`    \n"+
"                      :MMMM+                ````          yMMMNmNNNms                `.---.`                  `:smMMMNmdo.                        `.---.`       \n"+
"                      -MMMM:                              sMMMd+osss/                                            oMMMNs`                                        \n"+
"                      .NMMM-                              /MMMs                                    `-.           .+MMMh   ,,+ss.                                \n"+
"                       dMMN`                              .MMN-                                    yNNNNNNNmmmmNNmNNNNNNNMMMMMMm-                               \n"+
"                       sMMs                               `mMo                                     `-+hmmmhyso+:::-......-::+oo:                                \n"+
"                       sMo                                .Ns                                          ```                                                      \n"+
"                       --                                 `-                                                                                                    \n"+
"                                                                                                                                                                \n";
        for (; !stopSending; )
        {
            ConnectionManager.PutMessage(words, true, (error) => {sendTime = DateTime.Now.Ticks; });
            yield return new WaitForSeconds(10f);
        }
        yield break;
    }
}
