﻿using eAgenda.Dominio.ModuloTarefa;
using eAgenda.Infraestrutura.Compartilhado;
using eAgenda.Infraestrutura.ModuloTarefa;
using eAgenda.Infraestrutura.SqlServer.ModuloTarefa;
using eAgenda.WebApp.Extensions;
using eAgenda.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using static eAgenda.WebApp.Models.FormularioTarefaViewModel;

namespace eAgenda.WebApp.Controllers
{
    [Route("tarefas")]
    public class TarefaController : Controller
    {
        private readonly IRepositorioTarefa repositorioTarefa;

        public TarefaController()
        {
            repositorioTarefa = new RepositorioTarefaEmSql();
        }

        [HttpGet]
        public IActionResult Index(string? status)
        {
            List<Tarefa> registros;

            List<Tarefa> tarefas = repositorioTarefa.SelecionarTarefas();

            switch (status)
            {
                case "Pendente": registros = repositorioTarefa.SelecionarTarefasPendentes(); break;
                case "Concluído": registros = repositorioTarefa.SelecionarTarefasConcluidas(); break;
                default: registros = repositorioTarefa.SelecionarTarefas(); break;
            }

            var visualizarVM = new VisualizarTarefaViewModel(registros);

            return View(visualizarVM);
        }

        [HttpGet("cadastrar")]
        public IActionResult Cadastrar()
        {
            var cadastrarVM = new CadastrarTarefaViewModel();

            return View(cadastrarVM);
        }

        [HttpPost("cadastrar")]
        [ValidateAntiForgeryToken]
        public IActionResult Cadastrar(CadastrarTarefaViewModel cadastrarVM)
        {
            var registros = repositorioTarefa.SelecionarTarefas();

            foreach (var item in registros)
            {
                if (item.Titulo.Equals(cadastrarVM.Titulo))
                {
                    ModelState.AddModelError("CadastroUnico", "Já existe uma Tarefa registrado com este Titulo.");
                    break;
                }
            }

            for (int i = 0; i < cadastrarVM.TitulosItens.Count; i++)
            {
                var tituloItem = cadastrarVM.TitulosItens[i];

                if (string.IsNullOrWhiteSpace(tituloItem) || tituloItem.Length < 2 || tituloItem.Length > 100)
                {
                    ModelState.AddModelError($"TitulosItens[{i}]", $"O título do item {i + 1} deve ter entre 2 e 100 caracteres.");
                }
            }

            if (!ModelState.IsValid)
                return View(cadastrarVM);

            var entidade = cadastrarVM.ParaEntidade();

            repositorioTarefa.Cadastrar(entidade);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("editar/{id:guid}")]
        public IActionResult Editar(Guid id)
        {
            var tarefa = repositorioTarefa.SelecionarTarefaPorId(id);

            if (tarefa == null)
                return NotFound();

            var editarVM = new EditarTarefaViewModel(
                tarefa.Id,
                tarefa.Titulo,
                tarefa.Prioridade,
                tarefa.DataCriacao,
                tarefa.StatusConcluida,
                tarefa.PercentualConcluido,
                tarefa.DataConclusao,
                tarefa.Itens
            );

            return View(editarVM);


        }

        [HttpPost("editar/{id:guid}")]
        public IActionResult EditarConfirmado(Guid id, EditarTarefaViewModel editarVM)
        {
            for (int i = 0; i < editarVM.TitulosItens.Count; i++)
            {
                var tituloItem = editarVM.TitulosItens[i];

                if (string.IsNullOrWhiteSpace(tituloItem) || tituloItem.Length < 2 || tituloItem.Length > 100)
                {
                    ModelState.AddModelError($"TitulosItens[{i}]", $"O título do item {i + 1} deve ter entre 2 e 100 caracteres.");
                }
            }

            if (!ModelState.IsValid)
                return View("Editar", editarVM);

            var entidadeEditada = editarVM.ParaEntidade();

            repositorioTarefa.Editar(id, entidadeEditada);

            return RedirectToAction(nameof(Index));
        }


        [HttpGet("excluir/{id:guid}")]
        public IActionResult Excluir(Guid id)
        {
            var registroSelecionado = repositorioTarefa.SelecionarTarefaPorId(id);

            var excluirVM = new ExcluirTarefaViewModel(registroSelecionado.Id, registroSelecionado.Titulo);

            return View(excluirVM);
        }

        [HttpPost("excluir/{id:guid}")]
        public IActionResult ExcluirConfirmado(Guid id)
        {
            repositorioTarefa.Excluir(id);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("detalhes/{id:guid}")]
        public IActionResult Detalhes(Guid id)
        {
            var tarefa = repositorioTarefa.SelecionarTarefaPorId(id);

            if (tarefa == null)
                return NotFound();

            var detalhesVM = new DetalhesTarefaViewModel(
                tarefa.Id,
                tarefa.Titulo,
                tarefa.Prioridade,
                tarefa.DataCriacao,
                tarefa.StatusConcluida,
                tarefa.PercentualConcluido,
                tarefa.Itens,
                tarefa.DataConclusao
            );

            return View(detalhesVM);
        }
    }
}