using System;
using UnityEngine;

/// <summary>
/// Una linea de conversacion: quien la dice y que dice. Las conversaciones son
/// listas ordenadas de estas lineas (se avanzan con click) y la burbuja salta
/// al hablante de cada linea.
/// </summary>
[Serializable]
public class DialogueLine
{
    public enum Speaker { Amigo, Player }

    [Tooltip("Quien dice esta linea.")]
    public Speaker speaker = Speaker.Amigo;

    [TextArea]
    [Tooltip("Texto de la linea.")]
    public string text = "";
}
